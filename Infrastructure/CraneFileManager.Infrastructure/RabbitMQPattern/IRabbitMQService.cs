using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Infrastructure.RabbitMQPattern
{
    public interface IRabbitMQService
    {
        Task PublishMessage(string message, string exchange, string routingKey);

        Task PublishMessageAsCollection<T>(List<T> message, string exchange, string routingKey);

    }
}
