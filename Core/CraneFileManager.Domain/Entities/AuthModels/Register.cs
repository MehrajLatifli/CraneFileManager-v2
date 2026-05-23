using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Domain.Entities.AuthModels
{
    public class Register
    {

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
