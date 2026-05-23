using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Application.Mapper.DTO.UserClaimDTO
{
    public class UserClaimDTOforUpdate
    {
        [Required(ErrorMessage = "Id is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User Id is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "User Permition Id is required")]
        public Guid UserPermitionId { get; set; }
    }
}
