using CraneFileManager.Application.Mapper.DTO.NotificationDTO;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace CraneFileManager.Infrastructure.SignalR
{
    public class NotificationHubService
    {
        private readonly HubConnection _hubConnection;

        public bool IsConnected => _hubConnection.State == HubConnectionState.Connected;

        public NotificationHubService(string hubUrl)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .Build();

            _hubConnection.Closed += async (error) =>
            {
                Console.WriteLine("Connection closed. Attempting to reconnect...");
                await ReconnectAsync();
            };
        }

        public async Task StartAsync()
        {
            if (!IsConnected)
            {
                try
                {
                    await _hubConnection.StartAsync();
                    Console.WriteLine("Connected to the SignalR hub.");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to connect to the hub: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task SendMessageToUserAsync(string userId, NotificationDTOforGetandGetAll message)
        {
            if (IsConnected)
            {
                await _hubConnection.InvokeAsync("SendMessage", userId, message);
            }
            else
            {
                await StartAsync();
                if (IsConnected)
                {
                    await _hubConnection.InvokeAsync("SendMessage", userId, message);
                }
                else
                {
                    throw new InvalidOperationException("Unable to send message: connection is not active.");
                }
            }
        }

        private async Task ReconnectAsync()
        {
            while (!IsConnected)
            {
                try
                {
                    await _hubConnection.StartAsync();
                    Console.WriteLine("Reconnected to the SignalR hub.");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Reconnection attempt failed: {ex.Message}");
                    await Task.Delay(2000);
                }
            }
        }
    }
}
