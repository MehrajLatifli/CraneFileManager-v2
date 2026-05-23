namespace CraneFileManager.Application.Mapper.DTO.NotificationDTO
{
    public class NotificationDTOforGetandGetAll
    {

        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime? NotificationDate { get; set; }
    }
}
