using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Application.Mapper.DTO.UserPermissionDTO
{
    public class UserPermissionDTOforCreate
    {
        [Required(ErrorMessage = "Id  is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User Access  is required")]
        public string UserAccess { get; set; }

        [Required(ErrorMessage = "User Access Description  is required")]
        public string UserAccessDescription { get; set; }
    }
}
