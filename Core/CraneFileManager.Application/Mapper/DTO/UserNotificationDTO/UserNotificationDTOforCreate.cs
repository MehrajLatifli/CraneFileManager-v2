using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraneFileManager.Application.Mapper.DTO.UserNotificationDTO
{
    public class UserNotificationDTOforCreate
    {

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
