namespace CraneFileManager.Application.Mapper.DTO.UserNotificationDTO
{
    public class UserNotificationDTOforGetandGetAll
    {
        public Guid Id { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public Guid UserId { get; set; }

        public Guid NotificationId { get; set; }

    }
}
