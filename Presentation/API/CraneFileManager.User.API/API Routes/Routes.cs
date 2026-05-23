namespace CraneFileManager.User.API.API_Routes
{
    public struct Routes
    {
        #region Auth

        public const string Profile = "profile";
        public const string DeleteProfile = Profile + "/{id:guid}";
        public const string ProfilePassword = "profilePassword";
        public const string UserBlockStatus = "UserBlockStatus";
        public const string RegisterAdmin = "registerAdmin";
        public const string RegisterUser = "registerUser";
        public const string Login = "login";
        public const string Logout = "logout";
        public const string User = "user";
        public const string UserById = User + "/{id:guid}";
        public const string DeleteUser = User + "/{id:guid}";
        public const string RefreshToken = "refreshtoken";

        #endregion

        #region File


        public const string File = "file";
        public const string UpdateFile = File + "/{id:guid}";
        public const string FileTrashCan = "FileTrashCan";
        public const string AddFileTrashCan = FileTrashCan+"/{Id:guid}";
        public const string UpdateFileTrashCan = FileTrashCan + "/{Id:guid}";
        public const string GetFileById = File + "/{id:guid}";



        #endregion
    }
}
