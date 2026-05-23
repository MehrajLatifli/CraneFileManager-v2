using CraneFileManager.Application.Mapper.DTO.NotificationDTO;
using CraneFileManager.Infrastructure.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CraneFileManager.Notification.API.BackgroundServices
{
    public class NotificationBackgroundService : IHostedService, IDisposable
    {
        private static readonly ConcurrentBag<Channel<NotificationDTOforGetandGetAll>> _subscribers = new();
        private readonly NotificationHubService _notificationHubService;
        private readonly ILogger<NotificationBackgroundService> _logger;
        private Timer _timer;

        public NotificationBackgroundService(NotificationHubService notificationHubService, ILogger<NotificationBackgroundService> logger)
        {
            _notificationHubService = notificationHubService;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(SendMessageAsync, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
        }

        private async void SendMessageAsync(object state)
        {
            try
            {
       

                var messages = NotificationHub.GetReceivedMessages().ToList();


                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime localTime = localZone.ToLocalTime(DateTime.UtcNow);

                Task.Delay(1000);

                foreach (var item in messages)
                {
                    var message = new NotificationDTOforGetandGetAll
                    {
                        Id = item.Id,
                        Description = item.Description,
                        NotificationDate = localTime,
                        Title = item.Title
                    };

                    // Notify all subscribers
                    foreach (var subscriber in _subscribers)
                    {
                        await subscriber.Writer.WriteAsync(message);
                    }

                    _logger.LogInformation("Sent periodic notification message: {@Message}", message);
                }

                NotificationHub.ClearReceivedMessages(); // Clear after processing
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending periodic notification message.");
            }
        }

        public static Channel<NotificationDTOforGetandGetAll> Subscribe()
        {
            var channel = Channel.CreateUnbounded<NotificationDTOforGetandGetAll>();
            _subscribers.Add(channel);
            return channel;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            foreach (var subscriber in _subscribers)
            {
                subscriber.Writer.Complete(); // Mark all writers as complete
            }
        }
    }

}
