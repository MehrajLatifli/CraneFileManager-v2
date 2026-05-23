using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Application.Mapper.DTO.RoleDTO
{
    public class RoleDTOforUpdate
    {
        [Required(ErrorMessage = "Id  is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Role Name  is required")]
        public string Name { get; set; }
    }
}
