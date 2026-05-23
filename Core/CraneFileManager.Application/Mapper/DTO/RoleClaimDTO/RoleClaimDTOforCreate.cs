using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Mapper.DTO.RoleClaimDTO
{
    public class RoleClaimDTOforCreate
    {

        [Required(ErrorMessage = "Role Id is required")]
        public Guid RoleId { get; set; }

        [Required(ErrorMessage = "Role Permission Id is required")]
        public Guid RolePermissionId { get; set; }
    }
}
