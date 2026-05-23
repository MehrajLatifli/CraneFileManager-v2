using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Mapper.DTO.FileDTO
{
    public class FileDTOforCreate
    {
        [Required(ErrorMessage = "OrginalName is required")]
        public string OrginalName { get; set; }

        [Required(ErrorMessage = "DisplayName is required")]
        public string DisplayName { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Size is required")]
        public string Size { get; set; }

        [Required(ErrorMessage = "Path is required")]
        public string Path { get; set; }

        [Required(ErrorMessage = "IsRemove is required")]
        public bool? IsRemove { get; set; }

        [Required(ErrorMessage = "CreatedDate is required")]
        public DateTime? CreatedDate { get; set; }

        [Required(ErrorMessage = "UpdatedDate is required")]
        public DateTime? UpdatedDate { get; set; }

        [Required(ErrorMessage = "FileTypeId is required")]
        public Guid? FileTypeId { get; set; }
    }
}
