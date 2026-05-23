using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Application.Mapper.DTO.UserRoleDTO
{
    public class UserRoleDTOforUpdate
    {
        [Required(ErrorMessage = "Id  is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "UserId  is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "RoleId  is required")]
        public Guid RoleId { get; set; }

    }
}
