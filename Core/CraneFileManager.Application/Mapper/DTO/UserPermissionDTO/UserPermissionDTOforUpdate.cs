using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Mapper.DTO.UserPermissionDTO
{
    public class UserPermissionDTOforUpdate
    {
        [Required(ErrorMessage = "Id  is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User Access  is required")]
        public string UserAccess { get; set; }

        [Required(ErrorMessage = "User Access Description  is required")]
        public string UserAccessDescription { get; set; }
    }
}
