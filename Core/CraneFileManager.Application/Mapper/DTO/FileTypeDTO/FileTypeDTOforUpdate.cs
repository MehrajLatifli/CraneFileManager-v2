using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Application.Mapper.DTO.FileTypeDTO
{
    public class FileTypeDTOforUpdate
    {
        [Required(ErrorMessage = "Id is required")]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Type is required")]
        public string Type { get; set; }
        [Required(ErrorMessage = "CreatedDate is required")]
        public DateTime? CreatedDate { get; set; }
        [Required(ErrorMessage = "UpdatedDate is required")]
        public DateTime? UpdatedDate { get; set; }
    }
}
