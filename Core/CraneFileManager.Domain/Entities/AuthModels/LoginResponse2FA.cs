namespace CraneFileManager.Domain.Entities.AuthModels
{
    public class LoginResponse2FA
    {

        public string Token { get; set; }

        public string RefreshToken { get; set; }

        public string Expiration { get; set; }

        public string SecretKey { get; set; }
    }
}
