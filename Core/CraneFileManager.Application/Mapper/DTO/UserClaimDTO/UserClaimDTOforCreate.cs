using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Mapper.DTO.UserClaimDTO
{
    public class UserClaimDTOforCreate
    {
        [Required(ErrorMessage = "User Id is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "User Permition Id is required")]
        public Guid UserPermitionId { get; set; }
    }
}
