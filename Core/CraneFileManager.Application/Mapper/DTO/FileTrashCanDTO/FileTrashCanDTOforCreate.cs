using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Mapper.DTO.FileTrashCanDTO
{
    public class FileTrashCanDTOforCreate
    {
        [Required(ErrorMessage = "ThrowTrashDate is required")]
        public DateTime? ThrowTrashDate { get; set; }

        [Required(ErrorMessage = "TakeofTrashDate is required")]
        public DateTime? TakeofTrashDate { get; set; }

        [Required(ErrorMessage = "FileId is required")]
        public Guid? FileId { get; set; }
    }
}
