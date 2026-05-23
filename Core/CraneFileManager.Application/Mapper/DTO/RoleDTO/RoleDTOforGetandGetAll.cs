using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Application.Mapper.DTO.RoleDTO
{
    public class RoleDTOforGetandGetAll
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        
        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
