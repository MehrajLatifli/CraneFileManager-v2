using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Application.Mapper.DTO.FileDTO
{
    public class FileDTOforUpdate
    {
        [Required(ErrorMessage = "Id is required")]
        public Guid Id { get; set; }




        [Required(ErrorMessage = "DisplayName is required")]
        public string DisplayName { get; set; }



    }
}
