using CraneFileManager.Application.Validations;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Application.Mapper.DTO.FileDTO
{
    public class UploadFileDTO
    {

        [Required(ErrorMessage = "File is required")]
        [FileSizeForFileType(ErrorMessage = "Invalid file size or type.")]
        public IFormFile Name { get; set; }


    }
}
