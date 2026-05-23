using CraneFileManager.Application.Validations;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Mapper.DTO.UserDTO
{
    public class UserDTOforCreate
    {
        [Required(ErrorMessage = "User Name is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "User Email is required")]
        [Email(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "User Password is required")]
        [StrongPassword(ErrorMessage = "Minimum eight characters, at least one uppercase letter, one lowercase letter, one special character and one number")]
        public string Password { get; set; }

        [Required(ErrorMessage = "User Confirm password is required")]
        [StrongPassword(ErrorMessage = "Minimum eight characters, at least one uppercase letter, one lowercase letter, one special character and one number")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Birthday is required")]
        [AgeRestriction(14, ErrorMessage = "You must be at least 14 years old.")]
        public DateTime? Birthday { get; set; }

        [Required(ErrorMessage = "Profile Image is required")]
        [FileSize(5, 10)]
        public string ProfileImage { get; set; }


        [Required(ErrorMessage = "Created Date is required")]
        public DateTime? CreatedDate { get; set; }


        [Required(ErrorMessage = "IsBlcok is required")]
        public bool? IsBlcok { get; set; }


        [Required(ErrorMessage = "IsBlcok is required")]
        public bool? IsActive { get; set; }

        [Required(ErrorMessage = "RefreshToken is required")]
        public string RefreshToken { get; set; }

        [Required(ErrorMessage = "RefreshTokenExpiryTime is required")]
        public DateTime? RefreshTokenExpiryTime { get; set; }

        [Required(ErrorMessage = "SecretKey is required")]
        public string SecretKey { get; set; }


    }
}
