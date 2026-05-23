using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Mapper.DTO.RolePermissionDTO
{
    public class RolePermissionDTOforGetandGetAll
    {
        public Guid Id { get; set; }

        public string Method { get; set; }

        public string MethodDescription { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
