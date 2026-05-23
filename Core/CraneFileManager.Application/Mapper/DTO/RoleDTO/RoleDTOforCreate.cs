using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Mapper.DTO.RoleDTO
{
    public class RoleDTOforCreate
    {
        [Required(ErrorMessage = "Role Name  is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Created Date is required")]
        public DateTime? CreatedDate { get; set; }
    }
}
