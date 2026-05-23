namespace CraneFileManager.Application.Mapper.DTO.FileTypeDTO
{
    public class FileTypeDTOforGetandGetAll
    {
        public Guid Id { get; set; }

        public string Type { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
