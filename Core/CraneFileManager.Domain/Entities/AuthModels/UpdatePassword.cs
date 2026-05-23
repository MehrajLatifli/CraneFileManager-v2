using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Domain.Entities.AuthModels
{
    public class UpdatePassword
    {
        public Guid Id { get; set; }
        public string? OldPassword { get; set; }

        public string? NewPassword { get; set; }


    }

}
