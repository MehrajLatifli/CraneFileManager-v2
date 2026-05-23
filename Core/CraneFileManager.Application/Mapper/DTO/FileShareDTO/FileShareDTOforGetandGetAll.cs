namespace CraneFileManager.Application.Mapper.DTO.FileShareDTO
{
    public class FileShareDTOforGetandGetAll
    {
        public Guid Id { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public Guid? FileId { get; set; }

        public Guid? UserId { get; set; }
    }
}
