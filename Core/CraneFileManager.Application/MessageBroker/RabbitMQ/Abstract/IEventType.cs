using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.MessageBroker.RabbitMQ.Abstract
{
    public interface IEventType
    {

        public string? Eventname { get; set; }
        public string? Eventtype { get; set; }
    }
}
