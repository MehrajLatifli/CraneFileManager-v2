using Microsoft.AspNetCore.Http;

namespace CraneFileManager.Domain.Entities.AuthModels
{
    public class UpdateProfile
    {
        public Guid Id { get; set; }


        public string? Username { get; set; }

        public string? Name { get; set; }

        public string? Surname { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public string? ConfirmPassword { get; set; }

        public DateTime? Birthday { get; set; }

        public IFormFile ProfileImage { get; set; }

    }

}
