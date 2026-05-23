using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Application.Mapper.DTO.AuthDTO
{
    public class LoginDTO2FA
    {
        [Required(ErrorMessage = "User Name is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "User Password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "2FA Code is required")]
        public string TwoFactorCode { get; set; }
    }
}
