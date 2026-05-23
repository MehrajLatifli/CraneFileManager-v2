using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CraneFileManager.Application.Cache.RedisCachePatterns.Abstract;
using CraneFileManager.Application.Exceptions;
using CraneFileManager.Application.Mapper.DTO.FileDTO;
using CraneFileManager.Application.MessageBroker.RabbitMQ.Custom;
using CraneFileManager.Application.Services.Concrete;
using CraneFileManager.Persistence.ServiceExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;

namespace CraneFileManager.File.API.BackgroundServices.FileBackgroundServices
{
    public class FileCreateConsumeRabbitMQHostedService<T> : BackgroundService, IDisposable where T : FileEventType
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly IConfiguration _configuration;


        public FileCreateConsumeRabbitMQHostedService(
            ILoggerFactory loggerFactory,
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration
           )
        {
            _logger = loggerFactory.CreateLogger<FileCreateConsumeRabbitMQHostedService<T>>();
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;

            _cancellationTokenSource = new CancellationTokenSource(); // Initialize the CancellationTokenSource
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Combine cancellation tokens to handle external and internal cancellations
            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _cancellationTokenSource.Token).Token;

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"],
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"],
                Port = Convert.ToInt32(_configuration["RabbitMQ:Port"]),

            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "FileCreate_exchange", type: ExchangeType.Direct, autoDelete: false);
            _channel.QueueDeclare("FileCreate_queue", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind("FileCreate_queue", "FileCreate_exchange", "FileCreate_notification");
            _channel.BasicQos(0, 1, false);

            // Handle RabbitMQ connection shutdown
            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, eventArgs) =>
            {
                // Check for cancellation before processing the message
                linkedToken.ThrowIfCancellationRequested();

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var fileService = scope.ServiceProvider.GetRequiredService<FileServiceManager>();
                    var cacheServiceFileDTOforGetandGetAll = scope.ServiceProvider.GetRequiredService<IFileCacheService<FileDTOforGetandGetAll>>();

                    var body = eventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var fileEvent = JsonConvert.DeserializeObject<T>(message);

                    if (fileEvent.Eventtype == "FileCreate_notification" && fileEvent.IsIdentity && !string.IsNullOrEmpty(fileEvent.CurrentUser))
                    {
                        var CurrentUser = fileEvent.CurrentUser;

                        await DoInActive(stoppingToken);

                        await HandleMessage(message);

                        using (var stream = new MemoryStream(fileEvent.FileContent))
                        {
                            var uploadFileDto = new UploadFileDTO
                            {
                                Name = new FormFile(stream, 0, fileEvent.FileContent.Length, fileEvent.OrginalName, fileEvent.OrginalName)
                            };

                            string connectionStringAzure = _configuration["ConnectionAzureStorage"];
                            await fileService.CreateFile(connectionStringAzure, fileEvent.CurrentUser, uploadFileDto.Name.FileName, fileEvent.DisplayName, fileEvent.Size, fileEvent.Path);

                            await cacheServiceFileDTOforGetandGetAll.DeleteAllFilesByUser(fileEvent.CurrentUser);

                            var files = await fileService.ViewFiles(CurrentUser);
                            foreach (var item in files)
                            {
                                await cacheServiceFileDTOforGetandGetAll.AddFile(item.OrginalName, item);
                            }
                        }

                        _logger.LogInformation($"Received message: {message}");
                        _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                    }
                    else
                    {
                        await DoInPassive(stoppingToken);
                        _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                    }
                }
            };

            // Start consuming messages
            _channel.BasicConsume("FileCreate_queue", false, consumer);

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            await Task.CompletedTask;
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
            if (e.Initiator == ShutdownInitiator.Application)
            {
                _logger.LogInformation("RabbitMQ connection was closed by the application.");
            }
            else if (e.Initiator == ShutdownInitiator.Peer)
            {
                _logger.LogInformation("RabbitMQ connection was closed by the RabbitMQ broker.");
            }
            else
            {
                _logger.LogInformation("RabbitMQ connection was closed for unknown reasons.");
            }

            // You can perform cleanup here if needed, like disposing resources, notifying the system, etc.
            // For instance, you might want to retry the connection, or log additional information.
        }

        // Gracefully handle cancellation
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public override void Dispose()
        {
            // Ensure that all resources are properly disposed of
            _channel.Close();
            _connection.Close();
            _channel.Dispose();
            _connection.Dispose();
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            base.Dispose();
        }
    }

}
