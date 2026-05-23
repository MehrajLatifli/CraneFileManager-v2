using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CraneFileManager.Domain.Entities.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CraneFileManager.Infrastructure.SignalR
{
    public sealed class NotificationHub : Hub
    {
        private static readonly ConcurrentBag<Notification> _receivedMessages = new ConcurrentBag<Notification>();
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public async Task SendMessage(string userId, Notification message)
        {
            _receivedMessages.Add(message);

            if (string.IsNullOrEmpty(userId))
            {
                await Clients.All.SendAsync("ReceiveMessage", message.Id, message);
                _logger.LogInformation("Broadcasting message: {@Message}", message);
            }
            else
            {
                await Clients.User(userId).SendAsync("ReceiveMessage", message.Id, message);
                _logger.LogInformation("Sending message to user {UserId}: {@Message}", userId, message);
            }
        }

        public static IEnumerable<Notification> GetReceivedMessages() => _receivedMessages;

        public static void ClearReceivedMessages()
        {
            _receivedMessages.Clear(); // Clear all messages
        }

        // Other methods...
    }
}
