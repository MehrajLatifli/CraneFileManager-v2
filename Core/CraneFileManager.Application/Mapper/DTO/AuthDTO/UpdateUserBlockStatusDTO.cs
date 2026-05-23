using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Application.Mapper.DTO.AuthDTO
{
    public class UpdateUserBlockStatusDTO
    {
        [Required(ErrorMessage = "Id is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User IsBlcok is required")]
        public bool? IsBlcok { get; set; }
    }
}
