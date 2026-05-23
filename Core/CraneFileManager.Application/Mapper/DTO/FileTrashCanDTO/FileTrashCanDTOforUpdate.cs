using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Application.Mapper.DTO.FileTrashCanDTO
{
    public class FileTrashCanDTOforUpdate
    {
        [Required(ErrorMessage = "Id is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "ThrowTrashDate is required")]
        public DateTime? ThrowTrashDate { get; set; }

        [Required(ErrorMessage = "TakeofTrashDate is required")]
        public DateTime? TakeofTrashDate { get; set; }

        [Required(ErrorMessage = "FileId is required")]
        public Guid? FileId { get; set; }
    }
}
