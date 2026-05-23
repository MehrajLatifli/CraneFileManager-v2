namespace CraneFileManager.Application.Mapper.DTO.AuthDTO
{
    public class LoginResponseDTO2FA
    {
        public string Token { get; set; }

        public string RefreshToken { get; set; }

        public string Expiration { get; set; }

        public string SecretKey { get; set; }
    }
}
