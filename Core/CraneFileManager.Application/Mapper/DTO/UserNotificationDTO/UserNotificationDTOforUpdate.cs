using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Application.Mapper.DTO.UserNotificationDTO
{
    public class UserNotificationDTOforUpdate
    {
        [Required(ErrorMessage = "Id is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "CreatedDate is required")]
        public DateTime? CreatedDate { get; set; }

        [Required(ErrorMessage = "UpdatedDate is required")]
        public DateTime? UpdatedDate { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        public Guid NotificationId { get; set; }

    }
}
