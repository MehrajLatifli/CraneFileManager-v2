namespace CraneFileManager.Application.Mapper.DTO.UserClaimDTO
{
    public class UserClaimDTOforGetandGetAll
    {

        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid UserPermitionId { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
