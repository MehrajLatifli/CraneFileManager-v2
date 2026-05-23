using CraneFileManager.Application.Mapper.DTO.AuthDTO;
using CraneFileManager.Domain.Entities.AuthModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Services.Abstract
{
    public interface IAuthService
    {
        #region Auth service

        public Task RegisterAdmin(RegisterDTO model, string ConnectionStringAzure);

        public Task RegisterUser(RegisterDTO model, string ConnectionStringAzure);

        public Task<LoginResponse> Login(LoginDTO model);


        public Task<LoginResponse2FA> LoginWith2FA(LoginDTO2FA model);

        public Task<string> GenerateTotpSecretKey();

        public Task<string> GenerateTotpCode(string username);

  

        public Task<byte[]> Generate2FAQRCode(string username, string filepath);


        public Task<TokenModel> RefreshToken(TokenModel model, ClaimsPrincipal claimsPrincipal);

        public Task Logout(ClaimsPrincipal claimsPrincipal);

        public Task UpdateProfile(UpdateProfileDTO model, System.Security.Claims.ClaimsPrincipal claimsPrincipal, string ConnectionStringAzure);
        public Task UpdateUserBlock(UpdateUserBlockStatusDTO model, System.Security.Claims.ClaimsPrincipal claimsPrincipal);

        public Task UpdateProfilePassword(UpdatePasswordDTO model, ClaimsPrincipal claimsPrincipal);
        public Task DeleteProfile(Guid Id, ClaimsPrincipal claimsPrincipal);

        public Task DeleteUser(Guid Id,ClaimsPrincipal claimsPrincipal);

        public Task<List<GetUserDTOModel>> GetUsers(ClaimsPrincipal claimsPrincipal);

        public Task<GetUserDTOModel> GetUserById(Guid Id, ClaimsPrincipal claimsPrincipal);

        public Task<GetUserDTOModel> Profile(ClaimsPrincipal claimsPrincipal);


        #endregion
    }

}
