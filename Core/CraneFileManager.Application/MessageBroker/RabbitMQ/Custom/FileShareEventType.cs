using CraneFileManager.Application.MessageBroker.RabbitMQ.Abstract;

namespace CraneFileManager.Application.MessageBroker.RabbitMQ.Custom
{
    public class FileShareEventType : IEventType
    {

        public FileShareEventType()
        {

        }

        public Guid? Id { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public Guid? FileId { get; set; }

        public Guid? UserId { get; set; }


        public string? Eventname { get; set; }
        public string? Eventtype { get; set; }
    }
}
