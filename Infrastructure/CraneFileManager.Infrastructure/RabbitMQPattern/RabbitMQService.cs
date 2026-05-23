using CraneFileManager.Domain.Entities.Configurations;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.Extensions.Options; // Add this using directive

namespace CraneFileManager.Infrastructure.RabbitMQPattern
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly ConnectionFactory _factory;
        private readonly AppSettings _appSettings;

        // Change the constructor to accept IOptions<AppSettings>
        public RabbitMQService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value; // Retrieve the actual AppSettings object
            _factory = CreateConnectionFactory();
        }

        private ConnectionFactory CreateConnectionFactory()
        {
            return new ConnectionFactory
            {
                HostName = _appSettings.RabbitMQ.HostName,
                UserName = _appSettings.RabbitMQ.UserName,
                Password = _appSettings.RabbitMQ.Password,
                Port = int.Parse(_appSettings.RabbitMQ.Port.ToString()),
                DispatchConsumersAsync = bool.Parse(_appSettings.RabbitMQ.DispatchConsumersAsync.ToString())
            };
        }

        public async Task PublishMessage(string message, string exchange, string routingKey)
        {
            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                try
                {
                    await Task.Delay(500);
                    channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Direct);

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: exchange,
                        routingKey: routingKey,
                        basicProperties: null,
                        body: body);

                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error publishing message: {ex.Message}");
                }
            }
        }

        public async Task PublishMessageAsCollection<T>(List<T> message, string exchange, string routingKey)
        {
            ObservableCollection<T> collectionMessage = new ObservableCollection<T>(message);

            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Direct);

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(collectionMessage, Formatting.Indented));

                channel.BasicPublish(exchange: exchange,
                    routingKey: routingKey,
                    basicProperties: null,
                    body: body);
            }
        }
    }
}
