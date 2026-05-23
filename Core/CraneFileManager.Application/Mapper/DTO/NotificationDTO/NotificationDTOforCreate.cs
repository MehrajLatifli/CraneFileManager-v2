using CraneFileManager.Domain.Entities.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Mapper.DTO.NotificationDTO
{
    public class NotificationDTOforCreate
    {

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "NotificationDate is required")]
        public DateTime? NotificationDate { get; set; }
    }


}
