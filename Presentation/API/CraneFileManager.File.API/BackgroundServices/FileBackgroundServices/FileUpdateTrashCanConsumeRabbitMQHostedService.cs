using AutoMapper;
using Azure.Storage.Blobs;
using CraneFileManager.Application.Cache.RedisCachePatterns.Abstract;
using CraneFileManager.Application.Exceptions;
using CraneFileManager.Application.Mapper.DTO.AuthDTO;
using CraneFileManager.Application.Mapper.DTO.FileDTO;
using CraneFileManager.Application.Mapper.DTO.FileTrashCanDTO;
using CraneFileManager.Application.MessageBroker.RabbitMQ.Custom;
using CraneFileManager.Application.Services.Concrete;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CraneFileManager.File.API.BackgroundServices.FileBackgroundServices
{
    public class FileUpdateTrashCanConsumeRabbitMQHostedService<T> : BackgroundService, IDisposable where T : FileEventType
    {
        private readonly ILogger<FileUpdateTrashCanConsumeRabbitMQHostedService<T>> _logger;
        private IConnection _connection;
        private IModel _channel;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _configuration;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isConnectionOpen = false;

        public FileUpdateTrashCanConsumeRabbitMQHostedService(
            ILoggerFactory loggerFactory,
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<FileUpdateTrashCanConsumeRabbitMQHostedService<T>>();
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _cancellationTokenSource.Token).Token;

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _configuration["RabbitMQ:HostName"],
                    UserName = _configuration["RabbitMQ:UserName"],
                    Password = _configuration["RabbitMQ:Password"],
                    Port = Convert.ToInt32(_configuration["RabbitMQ:Port"]),
                };

                // Establish the connection and channel
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _isConnectionOpen = true;

                _channel.ExchangeDeclare("FileTrashCanUpdate_exchange", ExchangeType.Direct, autoDelete: false);
                _channel.QueueDeclare("FileTrashCanUpdate_queue", durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind("FileTrashCanUpdate_queue", "FileTrashCanUpdate_exchange", "FileTrashCanUpdate_notification");
                _channel.BasicQos(0, 1, false);

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, eventArgs) =>
                {
                    int retryCount = 0;
                    bool success = false;

                    linkedToken.ThrowIfCancellationRequested();

                    while (!success && retryCount < 5)  // Max retry count
                    {
                        try
                        {
                            using (var scope = _serviceScopeFactory.CreateScope())
                            {
                                var fileService = scope.ServiceProvider.GetRequiredService<FileServiceManager>(); 
                                var cacheServiceFileDTOforGetandGetAll = scope.ServiceProvider.GetRequiredService<IFileCacheService<FileDTOforGetandGetAll>>();
                                var cacheServiceFileTrashCanDTOforGetandGetAll = scope.ServiceProvider.GetRequiredService<IFileTrashCanCacheService<FileTrashCanDTOforGetandGetAll>>();

                                var body = eventArgs.Body.ToArray();
                                var message = Encoding.UTF8.GetString(body);
                                var fileTrashCanEvent = JsonConvert.DeserializeObject<T>(message);

                                if (fileTrashCanEvent?.Eventtype == "FileTrashCanUpdate_notification")
                                {
                                    var currentUser = fileTrashCanEvent?.CurrentUser;

                                    if (fileTrashCanEvent?.IsIdentity == true && !string.IsNullOrEmpty(currentUser))
                                    {
                                        await DoInActive(stoppingToken);
                                        await HandleMessage(message);

                                        await fileService.UpdateTrashCan(fileTrashCanEvent?.Id ?? Guid.Empty, _configuration["ConnectionAzureStorage"], currentUser);
                                        await cacheServiceFileDTOforGetandGetAll.DeleteAllFilesByUser(currentUser);
                                     
                                        _logger.LogInformation($"Processed message: {message}");
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Invalid event type or missing user identity.");
                                    }
                                }
                                else
                                {
                                    await DoInPassive(stoppingToken);
                                    _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                                    _logger.LogWarning($"Invalid event type: {fileTrashCanEvent?.Eventtype}");
                                }

                                // Acknowledge the message after successful processing
                                if (_channel.IsOpen)
                                {
                                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                                    success = true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            retryCount++;
                            _logger.LogError(ex, $"Error processing message. Retry attempt: {retryCount}");

                            // Exponential backoff for retries
                            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));

                            if (retryCount >= 5)
                            {
                                _logger.LogError($"Max retry attempts reached for message. Rejecting and not requeueing.");
                                if (_channel.IsOpen)
                                {
                                    _channel.BasicReject(eventArgs.DeliveryTag, requeue: false);  // Reject without requeuing
                                }
                            }
                            else
                            {
                                if (_channel.IsOpen)
                                {
                                    _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);  // Requeue for retry
                                }
                            }
                        }
                    }
                };

                // Consume messages from the queue
                _channel.BasicConsume("FileTrashCanUpdate_queue", false, consumer);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BackgroundService execution.");
            }
        }

        private async Task HandleMessage(string message)
        {
            _logger.LogInformation($"Consumer received: {message}");
        }

        private async Task DoInActive(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running in Active mode at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken); // Simulate work
        }

        private async Task DoInPassive(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running in Passive mode at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken); // Simulate idle
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            if (_isConnectionOpen)
            {
                _isConnectionOpen = false;
                _logger.LogInformation("RabbitMQ connection closed by the peer. Attempting reconnection...");
                // Reconnect logic can be added here if necessary
            }
            else
            {
                _logger.LogInformation("RabbitMQ connection was closed by the application.");
            }
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public override void Dispose()
        {
            // Ensure channel and connection are disposed properly
            try
            {
                if (_channel.IsOpen)
                {
                    _channel.Close();
                    _channel.Dispose();
                }

                if (_connection.IsOpen)
                {
                    _connection.Close();
                    _connection.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during resource disposal.");
            }

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            base.Dispose();
        }
    }
}
