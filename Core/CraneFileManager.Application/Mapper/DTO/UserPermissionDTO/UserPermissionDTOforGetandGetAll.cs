namespace CraneFileManager.Application.Mapper.DTO.UserPermissionDTO
{
    public class UserPermissionDTOforGetandGetAll
    {
        public Guid Id { get; set; }

        public string UserAccess { get; set; }

        public string UserAccessDescription { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
