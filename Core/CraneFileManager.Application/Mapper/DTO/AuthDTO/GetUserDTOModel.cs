namespace CraneFileManager.Application.Mapper.DTO.AuthDTO
{
    public class GetUserDTOModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        
        public string Surname { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public DateTime? Birthday { get; set; }

        public string ProfileImage { get; set; }

        public bool? IsBlcok { get; set; }

        public bool? IsActive { get; set; }

        public string CreatedDate { get; set; }

        public string UpdatedDate { get; set; }

        public List<PermissionDTO> Permitions { get; set; }

    }
}
