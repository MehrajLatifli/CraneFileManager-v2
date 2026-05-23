using AutoMapper;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CraneFileManager.Application.Cache.RedisCachePatterns.Abstract;
using CraneFileManager.Application.Exceptions;
using CraneFileManager.Application.Mapper.DTO.AuthDTO;
using CraneFileManager.Application.Mapper.DTO.FileDTO;
using CraneFileManager.Application.MessageBroker.RabbitMQ.Custom;
using CraneFileManager.Application.Services.Concrete;
using CraneFileManager.Persistence.ServiceExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Index.HPRtree;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CraneFileManager.File.API.BackgroundServices.FileBackgroundServices
{

        public class FileUpdateConsumeRabbitMQHostedService<T> : BackgroundService, IDisposable where T : FileEventType
        {
            private readonly ILogger<FileUpdateConsumeRabbitMQHostedService<T>> _logger;
            private IConnection _connection;
            private IModel _channel;
            private IServiceScopeFactory _serviceScopeFactory;
            private CancellationTokenSource _cancellationTokenSource;
            private readonly IConfiguration _configuration;
            private bool _isConnectionOpen = false;

            public FileUpdateConsumeRabbitMQHostedService(
                ILoggerFactory loggerFactory,
                IServiceScopeFactory serviceScopeFactory,
                IConfiguration configuration)
            {
                _logger = loggerFactory.CreateLogger<FileUpdateConsumeRabbitMQHostedService<T>>();
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

                    // Establish a new connection to RabbitMQ
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _isConnectionOpen = true;

                    // Declare exchange and queue
                    _channel.ExchangeDeclare(exchange: "FileUpdate_exchange", type: ExchangeType.Direct, autoDelete: false);
                    _channel.QueueDeclare("FileUpdate_queue", durable: true, exclusive: false, autoDelete: false);
                    _channel.QueueBind("FileUpdate_queue", "FileUpdate_exchange", "FileUpdate_notification");
                    _channel.BasicQos(0, 1, false); // Ensure only one message is processed at a time

                    // Register for connection shutdown events
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
                                // Process the message
                                using (var scope = _serviceScopeFactory.CreateScope())
                                {
                                    var fileService = scope.ServiceProvider.GetRequiredService<FileServiceManager>();
                                    var cacheServiceFileDTOforGetandGetAll = scope.ServiceProvider.GetRequiredService<IFileCacheService<FileDTOforGetandGetAll>>();

                                    var body = eventArgs.Body.ToArray();
                                    var message = Encoding.UTF8.GetString(body);
                                    var fileEvent = JsonConvert.DeserializeObject<T>(message);

                                    // Only process if the event type is valid
                                    if (fileEvent.Eventtype == "FileUpdate_notification" && fileEvent.IsIdentity && !string.IsNullOrEmpty(fileEvent.CurrentUser))
                                    {
                                        var CurrentUser = fileEvent.CurrentUser;

                                        // Perform the business logic
                                        await DoInActive(stoppingToken);
                                        await HandleMessage(message);

                                        await fileService.UpdateFile(fileEvent?.Id ?? Guid.Empty, fileEvent.DisplayName, _configuration["ConnectionAzureStorage"], fileEvent.CurrentUser);

                                        // Clear file cache after update
                                        await cacheServiceFileDTOforGetandGetAll.DeleteAllFilesByUser(fileEvent.CurrentUser);

                                        // Update cache with the newly updated files
                                        var files = await fileService.ViewFiles(CurrentUser);
                                        foreach (var item in files)
                                        {
                                            await cacheServiceFileDTOforGetandGetAll.AddFile(item.OrginalName, item);
                                        }

                                        // Acknowledge the message after processing
                                        _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                                        success = true;
                                    }
                                    else
                                    {
                                        // Reject the message if it is invalid
                                        await DoInPassive(stoppingToken);
                                        _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                retryCount++;
                                _logger.LogError(ex, $"Error processing message. Retry attempt: {retryCount}");

                                // Exponential backoff: increasing delay between retries
                                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));  // 2^retryCount seconds delay

                                if (retryCount >= 5)
                                {
                                    _logger.LogError($"Max retry attempts reached for message. Rejecting and not requeueing.");
                                    _channel.BasicReject(eventArgs.DeliveryTag, requeue: false);  // Reject without requeuing
                                }
                                else
                                {
                                    _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);  // Requeue for retry
                                }
                            }
                        }
                    };

                    _channel.BasicConsume("FileUpdate_queue", false, consumer);

                    consumer.Shutdown += OnConsumerShutdown;
                    consumer.Registered += OnConsumerRegistered;
                    consumer.Unregistered += OnConsumerUnregistered;
                    consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

                    // Keep the task running until cancellation token is triggered
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

            // Gracefully handle cancellation
            public void Cancel()
            {
                _cancellationTokenSource.Cancel();
            }

            // Dispose of the resources properly
            public override void Dispose()
            {
                _logger.LogInformation("Disposing RabbitMQ connection.");
                _channel?.Close();
                _connection?.Close();
                _channel?.Dispose();
                _connection?.Dispose();
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                base.Dispose();
            }
        }
    }


