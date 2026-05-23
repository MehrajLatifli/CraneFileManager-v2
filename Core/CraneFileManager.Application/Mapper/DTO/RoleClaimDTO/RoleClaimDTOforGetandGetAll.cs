namespace CraneFileManager.Application.Mapper.DTO.RoleClaimDTO
{
    public class RoleClaimDTOforGetandGetAll
    {
        public Guid Id { get; set; }

        public Guid RoleId { get; set; }

        public Guid RolePermissionId { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
