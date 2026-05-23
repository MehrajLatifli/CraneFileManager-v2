using CraneFileManager.Application.MessageBroker.RabbitMQ.Abstract;
using CraneFileManager.Domain.Entities.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraneFileManager.Application.MessageBroker.RabbitMQ.Custom
{
    public class FileTypeEventType : IEventType
    {

        public FileTypeEventType()
        {

        }

        public Guid? Id { get; set; }

        public string Type { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }


        public string? Eventname { get; set; }
        public string? Eventtype { get; set; }
    }
}
