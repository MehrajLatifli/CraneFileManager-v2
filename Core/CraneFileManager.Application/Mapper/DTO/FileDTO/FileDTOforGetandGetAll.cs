using AutoMapper.Configuration.Annotations;
using CraneFileManager.Domain;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace CraneFileManager.Application.Mapper.DTO.FileDTO
{
    public class FileDTOforGetandGetAll
    {
 
        public Guid Id { get; set; }

        public string OrginalName { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }
        public bool? IsRemove { get; set; }

        public string Size { get; set; }
 
        public string Path { get; set; }
 
        //public bool? IsRemove { get; set; }
 
        public DateTime? CreatedDate { get; set; }
 
        public DateTime? UpdatedDate { get; set; }

        //[NoMap]
        //[Ignore]
        [JsonIgnore]
        [XmlIgnore]
        public Guid? FileTypeId { get; set; }
        public string? FileType { get; set; }
    }
}
