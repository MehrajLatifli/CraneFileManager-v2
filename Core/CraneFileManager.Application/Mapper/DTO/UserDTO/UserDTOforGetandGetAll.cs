using Microsoft.AspNetCore.Http;

namespace CraneFileManager.Application.Mapper.DTO.UserDTO
{
    public class UserDTOforGetandGetAll
    {
        public Guid Id { get; set; }

        public string Username { get; set; }


        public string Name { get; set; }


        public string Surname { get; set; }


        public string Email { get; set; }
        
        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public DateTime? Birthday { get; set; }

        public string ProfileImage { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool? IsBlcok { get; set; }

        public bool? IsActive { get; set; }

        public string RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }

        public string SecretKey { get; set; }

    }
}
