using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Application.Mapper.DTO.RoleClaimDTO
{
    public class RoleClaimDTOforUpdate
    {
        [Required(ErrorMessage = "Id  is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Role Id is required")]
        public Guid RoleId { get; set; }

        [Required(ErrorMessage = "Role Permission Id is required")]
        public Guid RolePermissionId { get; set; }
    }
}
