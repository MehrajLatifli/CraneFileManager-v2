namespace CraneFileManager.Application.Mapper.DTO.UserFileDTO
{
    public class UserFileDTOforGetandGetAll
    {
        public Guid Id { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public Guid? FileId { get; set; }

        public Guid? UserId { get; set; }
    }

}
