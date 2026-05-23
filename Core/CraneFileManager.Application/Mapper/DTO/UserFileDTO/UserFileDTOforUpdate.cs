using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Application.Mapper.DTO.UserFileDTO
{
    public class UserFileDTOforUpdate
    {
        [Required(ErrorMessage = "Id is required")]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "CreatedDate is required")]
        public DateTime? CreatedDate { get; set; }
        [Required(ErrorMessage = "UpdatedDate is required")]
        public DateTime? UpdatedDate { get; set; }
        [Required(ErrorMessage = "FileId is required")]
        public Guid? FileId { get; set; }
        [Required(ErrorMessage = "UserId is required")]
        public Guid? UserId { get; set; }
    }

}
