namespace CraneFileManager.Domain.Entities.AuthModels
{
    public class Login2FA
    {

        public string Username { get; set; }

        public string Password { get; set; }

        public string SecretKey { get; set; }
    }
}
