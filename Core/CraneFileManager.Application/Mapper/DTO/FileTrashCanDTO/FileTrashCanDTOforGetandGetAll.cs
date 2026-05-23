namespace CraneFileManager.Application.Mapper.DTO.FileTrashCanDTO
{
    public class FileTrashCanDTOforGetandGetAll
    {
        public Guid Id { get; set; }

        public DateTime? ThrowTrashDate { get; set; }

        public DateTime? TakeofTrashDate { get; set; }

        public Guid? FileId { get; set; }
    }
}
