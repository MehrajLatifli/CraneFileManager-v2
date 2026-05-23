using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Mapper.DTO.UserFileDTO
{
    public class UserFileDTOforCreate
    {
    
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
