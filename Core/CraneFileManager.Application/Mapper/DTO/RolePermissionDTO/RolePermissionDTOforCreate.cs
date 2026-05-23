using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Application.Mapper.DTO.RolePermissionDTO
{
    public class RolePermissionDTOforCreate
    {

        [Required(ErrorMessage = "Method  is required")]
        public string Method { get; set; }

        [Required(ErrorMessage = "Method Description  is required")]
        public string MethodDescription { get; set; }

    }
}
