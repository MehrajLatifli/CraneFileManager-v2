using CraneFileManager.Application.Mapper.DTO.RoleClaimDTO;
using CraneFileManager.Application.Mapper.DTO.RoleDTO;
using CraneFileManager.Application.Mapper.DTO.RolePermissionDTO;
using CraneFileManager.Application.Mapper.DTO.UserClaimDTO;
using CraneFileManager.Application.Mapper.DTO.UserPermissionDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Mapper.DTO.AuthDTO
{

    public class PermissionDTO
    {

        public List<UserPermissionDTOforGetandGetAll> UserPermissions { get; set; }

        public List<RolePermissionDTOforGetandGetAll> RolePermissions { get; set; }

        public List<RoleDTOforGetandGetAll> Roles { get; set; }




    }
}
