using CraneFileManager.Application.MessageBroker.RabbitMQ.Abstract;

namespace CraneFileManager.Application.MessageBroker.RabbitMQ.Custom
{
    public class FileTrashCanEventType : IEventType
    {

        public FileTrashCanEventType()
        {

        }

        public Guid? Id { get; set; }

        public DateTime? ThrowTrashDate { get; set; }

        public DateTime? TakeofTrashDate { get; set; }

        public Guid? FileId { get; set; }
        public string? CurrentUser { get; set; }
        public bool IsIdentity { get; set; }

        public string? Eventname { get; set; }
        public string? Eventtype { get; set; }
    }
}
