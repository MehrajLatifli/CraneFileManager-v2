using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Application.Mapper.DTO.NotificationDTO
{
    public class NotificationDTOforUpdate
    {
        [Required(ErrorMessage = "Id is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "NotificationDate is required")]
        public DateTime? NotificationDate { get; set; }
    }
}
