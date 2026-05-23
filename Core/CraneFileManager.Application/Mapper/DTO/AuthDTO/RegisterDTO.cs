using CraneFileManager.Application.Validations;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Mapper.DTO.AuthDTO
{
    public class RegisterDTO
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
        [AgeRestriction(20, ErrorMessage = "You must be at least 20 years old.")]
        public DateTime? Birthday { get; set; }

        [Required(ErrorMessage = "Profile Image is required")]
        [AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png", ".gif" })]
        [FileSize(5, 10)]
        public IFormFile ProfileImage { get; set; }


    }
}
