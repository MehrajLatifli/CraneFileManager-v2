using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using CraneFileManager.Application.MessageBroker.RabbitMQ.Abstract;

namespace CraneFileManager.Application.MessageBroker.RabbitMQ.Custom
{
    public class FileEventType : IEventType
    {

        public FileEventType()
        {

        }

        public Guid? Id { get; set; }
        public string OrginalName { get; set; }

        public string DisplayName { get; set; }

        public string Path { get; set; }
        public long Size { get; set; }

        public string? CurrentUser { get; set; }
        public bool IsIdentity { get; set; }

        public byte[] FileContent { get; set; }

        public string? Eventname { get; set; }
        public string? Eventtype { get; set; }

        public int ChunkIndex { get; set; }
        public int TotalChunks { get; set; }

        public DateTime? UpdatedDate { get; set; }

    }
}
