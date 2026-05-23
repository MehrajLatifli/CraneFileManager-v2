using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Application.Mapper.DTO.RolePermissionDTO
{
    public class RolePermissionDTOforUpdate
    {
        [Required(ErrorMessage = "Id is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Method  is required")]
        public string Method { get; set; }

        [Required(ErrorMessage = "Method Description  is required")]
        public string MethodDescription { get; set; }

    }
}
