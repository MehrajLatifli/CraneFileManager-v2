using AutoMapper;
using Azure.Storage.Blobs;
using CraneFileManager.Application.Cache.RedisCachePatterns.Abstract;
using CraneFileManager.Application.Exceptions;
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
    public class FileAddTrashCanConsumeRabbitMQHostedService<T> : BackgroundService, IDisposable where T : FileEventType
    {
        private readonly ILogger<FileAddTrashCanConsumeRabbitMQHostedService<T>> _logger;
        private IConnection _connection;
        private IModel _channel;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _configuration;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isConnectionOpen = false;  // To track if the connection is open

        public FileAddTrashCanConsumeRabbitMQHostedService(
            ILoggerFactory loggerFactory,
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<FileAddTrashCanConsumeRabbitMQHostedService<T>>();
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

                _channel.ExchangeDeclare(exchange: "FileTrashCanCreate_exchange", type: ExchangeType.Direct, autoDelete: false);
                _channel.QueueDeclare("FileTrashCanCreate_queue", durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind("FileTrashCanCreate_queue", "FileTrashCanCreate_exchange", "FileTrashCanCreate_notification");
                _channel.BasicQos(0, 1, false);

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                // Gracefully handle cancellation

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

                                if (fileTrashCanEvent.Eventtype == "FileTrashCanCreate_notification")
                                {
                                    var currentUser = fileTrashCanEvent.CurrentUser;

                                    if (fileTrashCanEvent.IsIdentity && !string.IsNullOrEmpty(currentUser))
                                    {
                                        await DoInActive(stoppingToken);

                                        await HandleMessage(message);

                                        await fileService.AddTrashCan(fileTrashCanEvent?.Id ?? Guid.Empty, _configuration["ConnectionAzureStorage"], currentUser);

                                        await cacheServiceFileDTOforGetandGetAll.DeleteAllFilesByUser(currentUser);
                                    }
                                }
                                else
                                {
                                    await DoInPassive(stoppingToken);
                                    _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                                }
                            }

                            // Acknowledge the message if no error occurs
                            if (_channel.IsOpen)
                            {
                                _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                            }
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            retryCount++;
                            _logger.LogError(ex, $"Error processing message. Retry attempt: {retryCount}");

                            // Exponential backoff: increasing delay between retries
                            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));  // 2^retryCount seconds delay

                            if (retryCount >= 5)
                            {
                                _logger.LogError($"Max retry attempts reached for message. Rejecting and not requeuing.");
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
                _channel.BasicConsume("FileTrashCanCreate_queue", false, consumer);

                // Handle shutdown events
                consumer.Shutdown += OnConsumerShutdown;
                consumer.Registered += OnConsumerRegistered;
                consumer.Unregistered += OnConsumerUnregistered;
                consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                // Log and continue service execution
                _logger.LogError(ex, "Error in BackgroundService execution.");
            }
        }

        private async Task HandleMessage(string message)
        {
            _logger.LogInformation($"Consumer received {message}");
        }

        // Methods that handle RabbitMQ consumer events
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogInformation($"Consumer connection shutdown: {e.ReplyText}");
        }

        private void OnConsumerRegistered(object sender, ConsumerEventArgs e)
        {
            _logger.LogInformation("Consumer registered");
        }

        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e)
        {
            _logger.LogInformation("Consumer unregistered");
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e)
        {
            _logger.LogInformation("Consumer cancelled");
        }

        // Additional methods for logging and control flow
        private async Task DoInActive(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time} in {mode} mode", DateTimeOffset.Now, "Active");
            await Task.Delay(1000, stoppingToken);
        }

        private async Task DoInPassive(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time} in {mode} mode", DateTimeOffset.Now, "Passive");
            await Task.Delay(1000, stoppingToken);
        }

        // Gracefully handle connection shutdown
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogInformation($"RabbitMQ connection shutdown: {e.ReplyText}");
            _isConnectionOpen = false;
        }

        // Gracefully handle cancellation
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
