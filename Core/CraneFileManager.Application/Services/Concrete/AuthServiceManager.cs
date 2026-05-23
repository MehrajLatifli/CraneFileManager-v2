using AutoMapper;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Files.DataLake.Models;
using CraneFileManager.Application.Cache.RedisCachePatterns;
using CraneFileManager.Application.Cache.RedisCachePatterns.Abstract;
using CraneFileManager.Application.Exceptions;
using CraneFileManager.Application.Mapper.DTO.AuthDTO;
using CraneFileManager.Application.Mapper.DTO.NotificationDTO;
using CraneFileManager.Application.Mapper.DTO.RoleClaimDTO;
using CraneFileManager.Application.Mapper.DTO.RoleDTO;
using CraneFileManager.Application.Mapper.DTO.RolePermissionDTO;
using CraneFileManager.Application.Mapper.DTO.UserClaimDTO;
using CraneFileManager.Application.Mapper.DTO.UserDTO;
using CraneFileManager.Application.Mapper.DTO.UserPermissionDTO;
using CraneFileManager.Application.Mapper.DTO.UserRoleDTO;
using CraneFileManager.Application.RedisCachePatterns.Concrete;
using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Application.Services.Abstract;
using CraneFileManager.Domain.Entities.AuthModels;
using CraneFileManager.Domain.Entities.IdentityAuth;
using CraneFileManager.Domain.Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NetTopologySuite.Index.HPRtree;
using NodaTime;
using QRCoder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using OtpNet;
using System.Drawing;
using System.Globalization;


namespace CraneFileManager.Application.Services.Concrete
{
    public class AuthServiceManager : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IAuthCacheService<UserDTOforGetandGetAll> _cacheServiceUserDTOforGetandGetAll;
        private readonly IAuthCacheService<UserDTOforUpdate> _cacheServiceUserDTOforUpdate;
        private readonly IAuthCacheService<UserDTOforCreate> _cacheServiceUserDTOforCreate;
        private readonly IAuthCacheService<GetUserDTOModel> _cacheServiceGetAuthDTOModel;
        private readonly ILogger<AuthServiceManager> _logger;
        private readonly IUserWriteRepository _userWriteRepository;
        private readonly IUserReadRepository _userReadRepository;
        private readonly IUserClaimWriteRepository _userClaimWriteRepository;
        private readonly IUserClaimReadRepository _userClaimReadRepository;
        private readonly IRoleWriteRepository _roleWriteRepository;
        private readonly IRoleReadRepository _roleReadRepository;
        private readonly IRoleClaimWriteRepository _roleClaimWriteRepository;
        private readonly IRoleClaimReadRepository _roleClaimReadRepository;
        private readonly IUserRoleWriteRepository _userRoleWriteRepository;
        private readonly IUserRoleReadRepository _userRoleReadRepository;
        private readonly IUserPermissionWriteRepository _userPermissionWriteRepository;
        private readonly IUserPermissionReadRepository _userPermissionReadRepository;
        private readonly IRolePermissionWriteRepository _rolePermissionWriteRepository;
        private readonly IRolePermissionReadRepository _rolePermissionReadRepository;
        private readonly INotificationWriteRepository _notificationWriteRepository;
        private readonly INotificationReadRepository _notificationReadRepository;
        private readonly IUserNotificationWriteRepository _userNotificationWriteRepository;
        private readonly IUserNotificationReadRepository _userNotificationReadRepository;

        public AuthServiceManager(IConfiguration configuration, IMapper mapper, IAuthCacheService<UserDTOforGetandGetAll> cacheServiceUserDTOforGetandGetAll, IAuthCacheService<UserDTOforUpdate> cacheServiceUserDTOforUpdate, IAuthCacheService<UserDTOforCreate> cacheServiceUserDTOforCreate, IAuthCacheService<GetUserDTOModel> cacheServiceGetAuthDTOModel, ILogger<AuthServiceManager> logger, IUserWriteRepository userWriteRepository, IUserReadRepository userReadRepository, IUserClaimWriteRepository userClaimWriteRepository, IUserClaimReadRepository userClaimReadRepository, IRoleWriteRepository roleWriteRepository, IRoleReadRepository roleReadRepository, IRoleClaimWriteRepository roleClaimWriteRepository, IRoleClaimReadRepository roleClaimReadRepository, IUserRoleWriteRepository userRoleWriteRepository, IUserRoleReadRepository userRoleReadRepository, IUserPermissionWriteRepository userPermissionWriteRepository, IUserPermissionReadRepository userPermissionReadRepository, IRolePermissionWriteRepository rolePermissionWriteRepository, IRolePermissionReadRepository rolePermissionReadRepository, INotificationWriteRepository notificationWriteRepository, INotificationReadRepository notificationReadRepository, IUserNotificationWriteRepository userNotificationWriteRepository, IUserNotificationReadRepository userNotificationReadRepository)
        {
            _configuration = configuration;
            _mapper = mapper;
            _cacheServiceUserDTOforGetandGetAll = cacheServiceUserDTOforGetandGetAll;
            _cacheServiceUserDTOforUpdate = cacheServiceUserDTOforUpdate;
            _cacheServiceUserDTOforCreate = cacheServiceUserDTOforCreate;
            _cacheServiceGetAuthDTOModel = cacheServiceGetAuthDTOModel;
            _logger = logger;
            _userWriteRepository = userWriteRepository;
            _userReadRepository = userReadRepository;
            _userClaimWriteRepository = userClaimWriteRepository;
            _userClaimReadRepository = userClaimReadRepository;
            _roleWriteRepository = roleWriteRepository;
            _roleReadRepository = roleReadRepository;
            _roleClaimWriteRepository = roleClaimWriteRepository;
            _roleClaimReadRepository = roleClaimReadRepository;
            _userRoleWriteRepository = userRoleWriteRepository;
            _userRoleReadRepository = userRoleReadRepository;
            _userPermissionWriteRepository = userPermissionWriteRepository;
            _userPermissionReadRepository = userPermissionReadRepository;
            _rolePermissionWriteRepository = rolePermissionWriteRepository;
            _rolePermissionReadRepository = rolePermissionReadRepository;
            _notificationWriteRepository = notificationWriteRepository;
            _notificationReadRepository = notificationReadRepository;
            _userNotificationWriteRepository = userNotificationWriteRepository;
            _userNotificationReadRepository = userNotificationReadRepository;
        }

        public async Task<LoginResponse> Login(LoginDTO model)
        {
            var user = _mapper.Map<UserDTOforGetandGetAll>(_userReadRepository.GetAll(false).AsEnumerable().Where(i => i.Username == model.Username).FirstOrDefault());

            var passwordHash = PasswordComputeHash(model.Password, Environment.GetEnvironmentVariable("Salt"));

 
            if (user != null)
            {

                if (user.IsBlcok is true)
                {
                    throw new UnauthorizedException($"The user named {user.Username} has been blocked.");
                }


                if (user.Password != passwordHash)
                {
                    throw new UnauthorizedException("Username or password is incorrect.");
                }

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };



                var token = CreateToken(authClaims);
                var refreshToken = GenerateRefreshToken();

                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInHours"], out int refreshTokenValidityInHours);





                System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();

                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime localTime = localZone.ToLocalTime(DateTime.UtcNow);

                CraneFileManager.Domain.Entities.IdentityAuth.User user_ = _mapper.Map<CraneFileManager.Domain.Entities.IdentityAuth.User>(new UserDTOforUpdate
                {
                    Id = user.Id,
                    Username = user.Username,
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    Password = user.Password,
                    ConfirmPassword = user.ConfirmPassword,
                    Birthday = user.Birthday,
                    CreatedDate = user.CreatedDate,
                    UpdatedDate = user.UpdatedDate,
                    IsBlcok = false,
                    IsActive = true,
                    ProfileImage = user.ProfileImage,
                    SecretKey = user.SecretKey,
                    RefreshToken = refreshToken,
                    RefreshTokenExpiryTime = localTime.AddDays(refreshTokenValidityInHours),
                });

                _userWriteRepository.Update(_mapper.Map<CraneFileManager.Domain.Entities.IdentityAuth.User>(user_));
                await _userWriteRepository.SaveAsync();

                var userToUpdate = _mapper.Map<UserDTOforUpdate>(user_);
                await _cacheServiceUserDTOforUpdate.UpdateUser(user_.Username, userToUpdate);





                LoginResponseDTO loginResponse = new LoginResponseDTO
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo.ToLocalTime().ToString("dd-MMM-yyyy HH:mm:ss"),
                };


                var loginresult = _mapper.Map<LoginResponse>(loginResponse);





                #region NotificationNoCurrentUser

                await NotificationNoCurrentUser(localTime, "LogIn", "LogIn");

                #endregion


                return loginresult;


            }

            else
            {
                throw new UnauthorizedException("Username or password is incorrect.");

            }
        }


        public async Task<LoginResponse2FA> LoginWith2FA(LoginDTO2FA model)
        {
            var user = _mapper.Map<UserDTOforGetandGetAll>(_userReadRepository.GetAll(false).FirstOrDefault(i => i.Username == model.Username));

            var passwordHash = PasswordComputeHash(model.Password, Environment.GetEnvironmentVariable("Salt"));



            if (user != null)
            {
                if (user.IsBlcok is true)
                {
                    throw new UnauthorizedException($"The user named {user.Username} has been blocked.");
                }

                if (user.Password != passwordHash)
                {
                    throw new UnauthorizedException("Username or password is incorrect.");
                }

                var authClaims = new List<Claim>
                {
                  new Claim(ClaimTypes.Name, user.Username),
                  new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                var token = CreateToken(authClaims);
                var refreshToken = GenerateRefreshToken();
                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInHours"], out int refreshTokenValidityInHours);

                DateTime localTime = DateTime.UtcNow;

                CraneFileManager.Domain.Entities.IdentityAuth.User user_ = _mapper.Map<CraneFileManager.Domain.Entities.IdentityAuth.User>(new UserDTOforUpdate
                {
                    Id = user.Id,
                    Username = user.Username,
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    Password = user.Password,
                    ConfirmPassword = user.ConfirmPassword,
                    Birthday = user.Birthday,
                    CreatedDate = user.CreatedDate,
                    UpdatedDate = user.UpdatedDate,
                    IsBlcok = false,
                    IsActive = true,
                    ProfileImage = user.ProfileImage,
                    RefreshToken = refreshToken,
                    RefreshTokenExpiryTime = localTime.AddDays(refreshTokenValidityInHours),
                    SecretKey = user.SecretKey
                });


                if (!ValidateTwoFactorCode(user_.SecretKey, model.TwoFactorCode, user_.Username))
                {
                    throw new UnauthorizedException("Invalid two-factor code.");
                }

                _userWriteRepository.Update(user_);
                await _userWriteRepository.SaveAsync();

                var userToUpdate = _mapper.Map<UserDTOforUpdate>(user_);
                await _cacheServiceUserDTOforUpdate.UpdateUser(user_.Username, userToUpdate);




                LoginResponseDTO2FA loginResponse = new LoginResponseDTO2FA
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo.ToLocalTime().ToString("dd-MMM-yyyy HH:mm:ss"),
                    SecretKey = user_.SecretKey

                };



                #region NotificationNoCurrentUser

                await NotificationNoCurrentUser(localTime, "LoginWith2FA", "LoginWith2FA");

                #endregion

                return _mapper.Map<LoginResponse2FA>(loginResponse);
            }
            else
            {
                throw new UnauthorizedException("Username or password is incorrect.");
            }
        }


        public async Task<string> GenerateTotpCode(string username)
        {

            var user = _mapper.Map<UserDTOforGetandGetAll>(_userReadRepository.GetAll(false).FirstOrDefault(i => i.Username == username));


            if (user != null)
            {

                if (user.Username != username)
                {
                    throw new UnauthorizedException("Username is incorrect.");
                }
                if (string.IsNullOrEmpty(user.SecretKey))
                {
                    throw new UnauthorizedException("SecretKey is null or empty.");
                }
                var totp = new Totp(Base32Encoding.ToBytes(user.SecretKey));

                System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();

                return totp.ComputeTotp(DateTime.UtcNow);
            }
            else
            {
                throw new UnauthorizedException("User not found");
            }
        }


        private static Dictionary<string, HashSet<long>> usedTotpCodes = new Dictionary<string, HashSet<long>>();


        private bool ValidateTwoFactorCode(string secretKey, string code, string username)
        {

            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(code))
            {
                Console.WriteLine("Invalid secret key or code.");
                return false;
            }

            var totp = new Totp(Base32Encoding.ToBytes(secretKey), step: 30);


            var verificationWindow = new VerificationWindow(previous: 1, future: 1);


            bool isValid = totp.VerifyTotp(code, out long timeStepMatched, verificationWindow);

            if (isValid)
            {
                if (usedTotpCodes.TryGetValue(username, out var usedCodes))
                {
                    if (usedCodes.Contains(timeStepMatched))
                    {
                        Console.WriteLine("TOTP code has already been used.");
                        return false;
                    }
                }
                else
                {
                    usedTotpCodes[username] = new HashSet<long>();
                }

                usedTotpCodes[username].Add(timeStepMatched);
                Console.WriteLine($"TOTP code validated and marked as used for step: {timeStepMatched}");
            }

            return isValid;
        }





        public async Task<string> GenerateTotpSecretKey()
        {
            await Task.Delay(1); var key = KeyGeneration.GenerateRandomKey(200); return Base32Encoding.ToString(key);
        }


        public async Task<byte[]> Generate2FAQRCode(string username, string filepath)
        {

            var user = _mapper.Map<UserDTOforGetandGetAll>(_userReadRepository.GetAll(false).FirstOrDefault(i => i.Username == username));


            if (user != null)
            {
                if (user.Username != username)
                {
                    throw new UnauthorizedException("Username is incorrect.");
                }


                string qrCodeUri = await GenerateQRCodeUri(username, user.SecretKey);


                byte[] logoBytes = await System.IO.File.ReadAllBytesAsync(filepath);

                byte[] qrCodeImage = await Generate2FAQRCodeImageWithLogoAsync(qrCodeUri, logoBytes);


                return qrCodeImage;

            }
            else
            {
                throw new UnauthorizedException("User not found");
            }


        }

        public async Task<string> GenerateQRCodeUri(string username, string secretKey)
        {
            await Task.Delay(1);
            string issuer = "Crane File Manager"; return $"otpauth://totp/{issuer}:{username}?secret={secretKey}&issuer={issuer}&digits=6";
        }

        public async Task<string> GenerateQRCodeImage(string uri)
        {
            await Task.Delay(1);
            using (var qrGenerator = new QRCodeGenerator())
            {
                using (var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q))
                {
                    using (var qrCode = new PngByteQRCode(qrCodeData))
                    {
                        byte[] qrCodeImage = qrCode.GetGraphic(20);
                        return Convert.ToBase64String(qrCodeImage);
                    }
                }
            }
        }

        public async Task<byte[]> GenerateQRCodeImageAsPng(string uri)
        {
            await Task.Delay(1);
            using (var qrGenerator = new QRCodeGenerator())
            {
                using (var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q))
                {
                    using (var qrCode = new PngByteQRCode(qrCodeData))
                    {
                        return qrCode.GetGraphic(5, Color.DeepSkyBlue, Color.White, true);
                    }
                }
            }
        }

        private Color HexToColor(string hex)
        {
            hex = hex.Replace("#", string.Empty);

            byte a = 255; int startIndex = 0;

            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                startIndex = 2;
            }

            byte r = byte.Parse(hex.Substring(startIndex, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(startIndex + 2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(startIndex + 4, 2), System.Globalization.NumberStyles.HexNumber);

            return Color.FromArgb(a, r, g, b);
        }


        public async Task<byte[]> Generate2FAQRCodeImageWithLogoAsync(string uri, byte[] logoBytes)
        {
            await Task.Delay(1);
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new PngByteQRCode(qrCodeData))
            {
                Color foregroundColor = HexToColor("#00afd9");
                Color backgroundColor = HexToColor("#FFFFFF");

                var qrCodeImageBytes = qrCode.GetGraphic(5, foregroundColor, backgroundColor, true);

                using (var msQrCode = new MemoryStream(qrCodeImageBytes))
                using (var qrCodeBitmapOriginal = new Bitmap(msQrCode))
                {
                    Bitmap qrCodeBitmap = ConvertTo32bpp(qrCodeBitmapOriginal);
                    using (var graphics = Graphics.FromImage(qrCodeBitmap))
                    {
                        using (var msLogo = new MemoryStream(logoBytes))
                        using (var logoBitmapOriginal = new Bitmap(msLogo))
                        {
                            Bitmap logoBitmap = ConvertTo32bpp(logoBitmapOriginal);

                            int logoSize = qrCodeBitmap.Width / 2;
                            var logoPosition = new Rectangle(
                                (qrCodeBitmap.Width - logoSize) / 2,
                                (qrCodeBitmap.Height - logoSize) / 2,
                                logoSize,
                                logoSize
                            );

                            graphics.DrawImage(logoBitmap, logoPosition);
                        }
                    }

                    using (var msFinal = new MemoryStream())
                    {
                        qrCodeBitmap.Save(msFinal, System.Drawing.Imaging.ImageFormat.Png);
                        return msFinal.ToArray();
                    }
                }
            }
        }


        private Bitmap ConvertTo32bpp(Bitmap source)
        {
            if (source.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
                return new Bitmap(source);
            }

            var result = new Bitmap(source.Width, source.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(result))
            {
                g.DrawImage(source, new Rectangle(0, 0, result.Width, result.Height));
            }
            return result;
        }









        public async Task Logout(ClaimsPrincipal claimsPrincipal)
        {
            var userDTO = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false))
                .FirstOrDefault(i => i.Username == claimsPrincipal.Identity.Name);

            if (userDTO == null)
            {
                throw new BadHttpRequestException("Invalid user name");
            }

            var user = _mapper.Map<CraneFileManager.Domain.Entities.IdentityAuth.User>(userDTO);

            user.IsActive = false;
            user.RefreshToken = string.Empty;
            user.RefreshTokenExpiryTime = null;

            _userWriteRepository.Update(user);
            await _userWriteRepository.SaveAsync();

            var userToUpdate = _mapper.Map<UserDTOforUpdate>(user);
            await _cacheServiceUserDTOforUpdate.UpdateUser(user.Username, userToUpdate);


            #region NotificationNoCurrentUser

            await NotificationCurrentUser(user.Username, "LogOut", "LogOut");

            #endregion
        }


        public async Task<GetUserDTOModel> Profile(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.Identity.IsAuthenticated)
            {
                if (!_mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false)).AsEnumerable().Any(i => string.IsNullOrEmpty(i.RefreshToken) && i.Username == claimsPrincipal.Identity.Name))
                {
                    var currentUser = claimsPrincipal.Identity.Name;

                    var users = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false));
                    var roles = _mapper.Map<List<RoleDTOforGetandGetAll>>(_roleReadRepository.GetAll(false));
                    var userPermissions = _mapper.Map<List<UserPermissionDTOforGetandGetAll>>(_userPermissionReadRepository.GetAll(false));
                    var userRoles = _mapper.Map<List<UserRoleDTOforGetandGetAll>>(_userRoleReadRepository.GetAll(false));
                    var rolePermissions = _mapper.Map<List<RolePermissionDTOforGetandGetAll>>(_rolePermissionReadRepository.GetAll(false)).Distinct().ToList();
                    var roleClaims = _mapper.Map<List<RoleClaimDTOforGetandGetAll>>(_roleClaimReadRepository.GetAll(false)).Distinct().ToList();
                    var userClaims = _mapper.Map<List<UserClaimDTOforGetandGetAll>>(_userClaimReadRepository.GetAll(false));

                    var currentUserDTO = users.FirstOrDefault(u => u.Username == currentUser);
                    if (currentUserDTO == null)
                    {
                        throw new NotFoundException("User not found.");
                    }

                    var permission = new PermissionDTO
                    {
                        UserPermissions = userPermissions.ToList(),
                        RolePermissions = rolePermissions.Where(rp => roleClaims.Any(rc => rc.RolePermissionId == rp.Id && userRoles.Any(ur => ur.RoleId == rc.RoleId && ur.UserId == currentUserDTO.Id))).ToList(),
                        Roles = roles.Where(r => userRoles.Any(ur => ur.RoleId == r.Id && ur.UserId == currentUserDTO.Id)).ToList()
                    };

                    var authResult = new GetUserDTOModel
                    {
                        Id = currentUserDTO.Id.ToString(),
                        Username = currentUserDTO.Username,
                        Name = currentUserDTO.Name,
                        Surname = currentUserDTO.Surname,
                        Email = currentUserDTO.Email,
                        Password = currentUserDTO.Password,
                        ConfirmPassword = currentUserDTO.ConfirmPassword,
                        Birthday = currentUserDTO.Birthday,
                        CreatedDate = currentUserDTO.CreatedDate?.ToString(),
                        UpdatedDate = currentUserDTO.UpdatedDate?.ToString(),
                        IsBlcok = currentUserDTO.IsBlcok,
                        IsActive = currentUserDTO.IsActive,
                        ProfileImage = currentUserDTO.ProfileImage,
                        Permitions = new List<PermissionDTO> { permission }
                    };

                    var cachedUser = await _cacheServiceGetAuthDTOModel.GetProfile(authResult.Username);
                    if (cachedUser == null)
                    {
                        var GetAuthDTOModel = _mapper.Map<GetUserDTOModel>(authResult);
                        await _cacheServiceGetAuthDTOModel.AddProfile(authResult.Username, GetAuthDTOModel);
                    }







                    #region NotificationCurrentUser


                    await NotificationCurrentUser(currentUser, "Get Profile", "View Profile");

                    #endregion

                    return await _cacheServiceGetAuthDTOModel.GetProfile(authResult.Username);
                }
                else
                {
                    throw new UnauthorizedException("Current user is not authenticated.");
                }
            }
            else
            {
                throw new UnauthorizedException("Current user is not authenticated.");
            }
        }

        public async Task<List<GetUserDTOModel>> GetUsers(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.Identity.IsAuthenticated)
            {
                var users = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false));
                var roles = _mapper.Map<List<RoleDTOforGetandGetAll>>(_roleReadRepository.GetAll(false));
                var userPermissions = _mapper.Map<List<UserPermissionDTOforGetandGetAll>>(_userPermissionReadRepository.GetAll(false));
                var userRoles = _mapper.Map<List<UserRoleDTOforGetandGetAll>>(_userRoleReadRepository.GetAll(false));
                var rolePermissions = _mapper.Map<List<RolePermissionDTOforGetandGetAll>>(_rolePermissionReadRepository.GetAll(false)).Distinct().ToList();
                var roleClaims = _mapper.Map<List<RoleClaimDTOforGetandGetAll>>(_roleClaimReadRepository.GetAll(false)).Distinct().ToList();

                var allUserDTOModels = new List<GetUserDTOModel>();

                foreach (var user in users)
                {
                    var userRolesForCurrentUser = userRoles.Where(ur => ur.UserId == user.Id).ToList();

                    var permission = new PermissionDTO
                    {
                        UserPermissions = userPermissions.ToList(),
                        RolePermissions = rolePermissions.Where(rp => roleClaims.Any(rc => rc.RolePermissionId == rp.Id &&
                            userRolesForCurrentUser.Any(ur => ur.RoleId == rc.RoleId))).ToList(),
                        Roles = roles.Where(r => userRolesForCurrentUser.Any(ur => ur.RoleId == r.Id)).ToList()
                    };

                    var userDTO = new GetUserDTOModel
                    {
                        Id = user.Id.ToString(),
                        Username = user.Username,
                        Name = user.Name,
                        Surname = user.Surname,
                        Email = user.Email,
                        Password = user.Password,
                        ConfirmPassword = user.ConfirmPassword,
                        Birthday = user.Birthday,
                        CreatedDate = user.CreatedDate?.ToString(),
                        UpdatedDate = user.UpdatedDate?.ToString(),
                        IsBlcok = user.IsBlcok,
                        IsActive = user.IsActive,
                        ProfileImage = user.ProfileImage,
                        Permitions = new List<PermissionDTO> { permission }
                    };

                    allUserDTOModels.Add(userDTO);
                }

                var cachedUsers = await _cacheServiceUserDTOforGetandGetAll.GetAllUsers();

                if (cachedUsers.Count <= 0)
                {
                    foreach (var item in allUserDTOModels)
                    {
                        await _cacheServiceGetAuthDTOModel.AddUser(item.Username, item);
                    }
                }

                #region NotificationCurrentUser


                await NotificationCurrentUser(claimsPrincipal.Identity.Name, "Get Users", "View Users");

                #endregion

                return allUserDTOModels;
            }
            else
            {
                throw new UnauthorizedException("Current user is not authenticated.");
            }
        }

        public async Task<TokenModel> RefreshToken(TokenModel tokenModel, ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.Identity.IsAuthenticated)
            {
                if (tokenModel is null)
                {

                    throw new BadHttpRequestException("Invalid client request1");

                }

                string? accessToken = tokenModel.AccessToken;
                string? refreshToken = tokenModel.RefreshToken;

                var principal = GetPrincipalFromExpiredToken(accessToken);
                if (principal == null)
                {

                    throw new BadHttpRequestException("Invalid access token or refresh token");

                }

                string username = principal.Identity.Name;


                var user = await _userReadRepository.GetSingleAsync(i => i.Username == username);

                if (user == null || user.RefreshToken != refreshToken)
                {

                    throw new BadHttpRequestException("Invalid access token or refresh token");

                }

                var newAccessToken = CreateToken(principal.Claims.ToList());
                var newRefreshToken = GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;
                _userWriteRepository.Update(user);

                await _userWriteRepository.SaveAsync();

                TokenModel model = new TokenModel
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                    RefreshToken = newRefreshToken
                };


                var userToUpdate = _mapper.Map<UserDTOforUpdate>(user);
                await _cacheServiceUserDTOforUpdate.UpdateUser(user.Username, userToUpdate);

                return model;

            }
            else
            {
                throw new UnauthorizedException("Current user is not authenticated.");
            }



        }

        public async Task RegisterAdmin(RegisterDTO model, string connectionStringAzure)
        {



            var userExists = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false))
                .Any(i => i.Username == model.Username);

            if (userExists)
            {
                throw new ConflictException("User already exists!");
            }

            if (model.ConfirmPassword != model.Password)
            {
                throw new ConflictException("Confirm Password does not match.");
            }

            string connectionString = GetAzureConnectionString(connectionStringAzure);
            string containerName = "profile-images";
            string userFolder = $"{model.Username}/";
            string blobName = $"{userFolder}{model.Username}_{Guid.NewGuid()}{Path.GetExtension(model.ProfileImage.FileName)}";

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = GetContentType(Path.GetExtension(model.ProfileImage.FileName)),
                ContentDisposition = "inline"
            };

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(blobName);
            using (var stream = model.ProfileImage.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
            }

            string imageUrl = blobClient.Uri.ToString();


            System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();

            TimeZone localZone = TimeZone.CurrentTimeZone;
            DateTime localTime = localZone.ToLocalTime(DateTime.UtcNow);


            var user = _mapper.Map<CraneFileManager.Domain.Entities.IdentityAuth.User>(new UserDTOforCreate
            {
                Username = model.Username,
                Name = model.Name,
                Surname = model.Surname,
                Email = model.Email,
                Password = PasswordComputeHash(model.Password, Environment.GetEnvironmentVariable("Salt")),
                ConfirmPassword = PasswordComputeHash(model.ConfirmPassword, Environment.GetEnvironmentVariable("Salt")),
                Birthday = model.Birthday,
                CreatedDate = localTime,
                IsBlcok = false,
                IsActive = false,
                ProfileImage = imageUrl,
                RefreshToken = string.Empty,
                RefreshTokenExpiryTime = null,
                SecretKey = await GenerateTotpSecretKey(),

            });

            var roleExists = _mapper.Map<List<RoleDTOforGetandGetAll>>(_roleReadRepository.GetAll(false))
                .Any(i => i.Name == UserRoles.Admin);

            try
            {


                if (!roleExists)
                {

                    await _userWriteRepository.AddAsync(user);
                    var userResult = await _userWriteRepository.SaveAsync();


                    var userToAdd = _mapper.Map<UserDTOforCreate>(user);
                    await _cacheServiceUserDTOforCreate.AddUser(user.Username, userToAdd);


                    if (userResult == -1)
                    {
                        throw new InvalidOperationException("Failed to create the user.");
                    }

                    var newRole = _mapper.Map<Role>(new RoleDTOforCreate
                    {
                        Name = UserRoles.Admin,
                        CreatedDate = localTime
                    });

                    await _roleWriteRepository.AddAsync(newRole);
                    var roleResult = await _roleWriteRepository.SaveAsync();
                    if (roleResult == -1)
                    {
                        throw new InvalidOperationException("Failed to create the role.");
                    }
                }
                else
                {
                    throw new ConflictException("Role already exists");
                }

                var roleId = _mapper.Map<List<RoleDTOforGetandGetAll>>(_roleReadRepository.GetAll(false))
                    .Where(i => i.Name == UserRoles.Admin)
                    .Select(i => i.Id)
                    .FirstOrDefault();

                var userId = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false))
                    .Where(i => i.Username == model.Username)
                    .Select(i => i.Id)
                    .FirstOrDefault();

                if (roleId == Guid.Empty || userId == Guid.Empty)
                {
                    throw new ConflictException("Role or User does not exist.");
                }

                var userRole = _mapper.Map<UserRole>(new UserRoleDTOforCreate
                {
                    UserId = userId,
                    RoleId = roleId,
                    CreatedDate = localTime
                });

                await _userRoleWriteRepository.AddAsync(userRole);
                var userRoleResult = await _userRoleWriteRepository.SaveAsync();
                if (userRoleResult == -1)
                {
                    throw new InvalidOperationException("Failed to create the UserRole.");
                }


                if (_userRoleReadRepository.GetAll(false).Where(i => i.RoleId == _roleReadRepository.GetAll(false).FirstOrDefault(i => i.Name == UserRoles.Admin).Id).Count() <= 1)
                {
                    if (_roleReadRepository.GetAll(false).Any(i => i.Name == UserRoles.Admin))
                    {
                        var rolePermission = _mapper.Map<List<RolePermission>>(GetRolePermissionsForAdmin(localTime));
                        var existingRolePermissions = _rolePermissionReadRepository.GetAll(false).ToList();
                        var missingPermissions = rolePermission.Except(existingRolePermissions, new RolePermissionComparer()).ToList();

                        foreach (var item in missingPermissions)
                        {
                            var rolePermissionEntity = _rolePermissionReadRepository.GetAll(false).FirstOrDefault(rp => rp.Id == item.Id);
                            if (rolePermissionEntity != null)
                            {
                                _rolePermissionWriteRepository.Remove(rolePermissionEntity);
                            }
                            else
                            {
                                await _rolePermissionWriteRepository.AddAsync(_mapper.Map<RolePermission>(item));
                            }

                            var rolePermissionResult = await _rolePermissionWriteRepository.SaveAsync();
                            if (rolePermissionResult == -1)
                            {
                                throw new InvalidOperationException("Failed to create the RolePermission.");
                            }
                        }

                        var roleClaims = _mapper.Map<List<RoleClaimDTOforGetandGetAll>>(missingPermissions.Select(permission => new RoleClaimDTOforGetandGetAll
                        {
                            RolePermissionId = permission.Id,
                            RoleId = roleId,
                            CreatedDate = localTime
                        }));

                        foreach (var item in roleClaims)
                        {
                            var roleClaimEntity = _roleClaimReadRepository.GetAll(false).FirstOrDefault(rc => rc.RolePermissionId == item.RolePermissionId && rc.RoleId == item.RoleId);
                            if (roleClaimEntity != null)
                            {
                                _roleClaimWriteRepository.Remove(roleClaimEntity);
                            }
                            else
                            {
                                await _roleClaimWriteRepository.AddAsync(_mapper.Map<RoleClaim>(item));
                            }

                            var roleClaimResult = await _roleClaimWriteRepository.SaveAsync();
                            if (roleClaimResult == -1)
                            {
                                throw new InvalidOperationException("Failed to create the RoleClaims.");
                            }
                        }
                    }
                }

                if (_mapper.Map<List<UserPermissionDTOforGetandGetAll>>(_userPermissionReadRepository.GetAll(false)).Select(rp => rp.UserAccess).OrderBy(UserAccess => UserAccess).ToList().SequenceEqual(GetUserPermissions(localTime).Select(rp => rp.UserAccess).OrderBy(username => username).ToList()) == false)
                {
                    List<UserPermission> defaultPermissions = GetUserPermissions(localTime);
                    await _userPermissionWriteRepository.AddRangeAsync(defaultPermissions);

                    var userPermissionResult = await _userPermissionWriteRepository.SaveAsync();
                    if (userPermissionResult == -1)
                    {
                        throw new InvalidOperationException("Failed to create the UserPermissions.");
                    }


                    try
                    {
                        var userClaims = _mapper.Map<List<UserPermissionDTOforGetandGetAll>>(_userPermissionReadRepository.GetAll(false)).ToList().Select(permission => new UserClaimDTOforGetandGetAll
                        {
                            UserPermitionId = permission.Id,
                            UserId = userId,
                            CreatedDate = localTime
                        }).ToList();

                        await _userClaimWriteRepository.AddRangeAsync(_mapper.Map<List<UserClaim>>(userClaims));
                        var userClaimResult = await _userClaimWriteRepository.SaveAsync();
                        if (userClaimResult == -1)
                        {
                            throw new InvalidOperationException("Failed to create the UserClaims.");
                        }

                        #region NotificationCurrentUser


                        await NotificationCurrentUser(model.Username, "Post Admin", "Create Admin");

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("An error occurred while saving user claims.");
                    }


                }

            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine($"Database update error: {ex.InnerException?.Message}");
                throw new InvalidOperationException(ex.InnerException?.Message);
            }
        }


        public async Task RegisterUser(RegisterDTO model, string connectionStringAzure)
        {



            var userExists = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false))
                .Any(i => i.Username == model.Username);

            if (userExists)
            {
                throw new ConflictException("User already exists!");
            }

            if (model.ConfirmPassword != model.Password)
            {
                throw new ConflictException("Confirm Password does not match.");
            }

            string connectionString = GetAzureConnectionString(connectionStringAzure);
            string containerName = "profile-images";
            string userFolder = $"{model.Username}/";
            string blobName = $"{userFolder}{model.Username}_{Guid.NewGuid()}{Path.GetExtension(model.ProfileImage.FileName)}";

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = GetContentType(Path.GetExtension(model.ProfileImage.FileName)),
                ContentDisposition = "inline"
            };

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(blobName);
            using (var stream = model.ProfileImage.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
            }

            string imageUrl = blobClient.Uri.ToString();

            System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();

            TimeZone localZone = TimeZone.CurrentTimeZone;
            DateTime localTime = localZone.ToLocalTime(DateTime.UtcNow);


            var user = _mapper.Map<CraneFileManager.Domain.Entities.IdentityAuth.User>(new UserDTOforCreate
            {
                Username = model.Username,
                Name = model.Name,
                Surname = model.Surname,
                Email = model.Email,
                Password = PasswordComputeHash(model.Password, Environment.GetEnvironmentVariable("Salt")),
                ConfirmPassword = PasswordComputeHash(model.ConfirmPassword, Environment.GetEnvironmentVariable("Salt")),
                Birthday = model.Birthday,
                CreatedDate = localTime,
                IsBlcok = false,
                IsActive = false,
                ProfileImage = imageUrl,
                RefreshToken = string.Empty,
                RefreshTokenExpiryTime = null,
                SecretKey = await GenerateTotpSecretKey(),
            });

            var roleExists = _roleReadRepository.GetAll(false)
                .Any(i => i.Name == UserRoles.User);


            await _userWriteRepository.AddAsync(user);
            var userResult = await _userWriteRepository.SaveAsync();



            var userToAdd = _mapper.Map<UserDTOforCreate>(user);
            await _cacheServiceUserDTOforCreate.AddUser(user.Username, userToAdd);

            if (userResult == -1)
            {
                throw new InvalidOperationException("Failed to create the user.");
            }

            if (!roleExists)
            {

                var newRole = new Role
                {
                    Name = UserRoles.User,
                    CreatedDate = localTime
                };

                await _roleWriteRepository.AddAsync(newRole);
                var roleResult = await _roleWriteRepository.SaveAsync();
                if (roleResult == -1)
                {
                    throw new InvalidOperationException("Failed to create the role.");
                }
            }

            var roleId = _mapper.Map<List<RoleDTOforGetandGetAll>>(_roleReadRepository.GetAll(false))
                .Where(i => i.Name == UserRoles.User)
                .Select(i => i.Id)
                .FirstOrDefault();

            var userId = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false))
                .Where(i => i.Username == model.Username)
                .Select(i => i.Id)
                .FirstOrDefault();

            if (roleId == Guid.Empty || userId == Guid.Empty)
            {
                throw new ConflictException("Role or User does not exist.");
            }

            var userRole = _mapper.Map<UserRole>(new UserRoleDTOforCreate
            {
                UserId = userId,
                RoleId = roleId,
                CreatedDate = localTime
            });

            await _userRoleWriteRepository.AddAsync(userRole);
            var userRoleResult = await _userRoleWriteRepository.SaveAsync();
            if (userRoleResult == -1)
            {
                throw new InvalidOperationException("Failed to create the UserRole.");
            }





            if (_userRoleReadRepository.GetAll(false).Where(i => i.RoleId == _roleReadRepository.GetAll(false).FirstOrDefault(i => i.Name == UserRoles.User).Id).Count() <= 1)
            {
                if (_roleReadRepository.GetAll(false).Any(i => i.Name == UserRoles.User))
                {

                    var rolePermission = _mapper.Map<List<RolePermission>>(GetRolePermissionsForUser(localTime));
                    var existingRolePermissions = _rolePermissionReadRepository.GetAll(false).ToList();
                    var missingPermissions = rolePermission.Except(existingRolePermissions, new RolePermissionComparer()).ToList();

                    foreach (var item in missingPermissions)
                    {
                        var rolePermissionEntity = _rolePermissionReadRepository.GetAll(false).FirstOrDefault(rp => rp.Id == item.Id);
                        if (rolePermissionEntity != null)
                        {
                            _rolePermissionWriteRepository.Remove(rolePermissionEntity);
                        }
                        else
                        {
                            await _rolePermissionWriteRepository.AddAsync(_mapper.Map<RolePermission>(item));
                        }

                        var rolePermissionResult = await _rolePermissionWriteRepository.SaveAsync();
                        if (rolePermissionResult == -1)
                        {
                            throw new InvalidOperationException("Failed to create the RolePermission.");
                        }
                    }

                    var roleClaims = _mapper.Map<List<RoleClaimDTOforGetandGetAll>>(missingPermissions.Select(permission => new RoleClaimDTOforGetandGetAll
                    {
                        RolePermissionId = permission.Id,
                        RoleId = roleId,
                        CreatedDate = localTime
                    }));

                    foreach (var item in roleClaims)
                    {
                        var roleClaimEntity = _roleClaimReadRepository.GetAll(false).FirstOrDefault(rc => rc.RolePermissionId == item.RolePermissionId && rc.RoleId == item.RoleId);
                        if (roleClaimEntity != null)
                        {
                            _roleClaimWriteRepository.Remove(roleClaimEntity);
                        }
                        else
                        {
                            await _roleClaimWriteRepository.AddAsync(_mapper.Map<RoleClaim>(item));
                        }

                        var roleClaimResult = await _roleClaimWriteRepository.SaveAsync();
                        if (roleClaimResult == -1)
                        {
                            throw new InvalidOperationException("Failed to create the RoleClaims.");
                        }
                    }
                }
            }

            if (_mapper.Map<List<UserPermissionDTOforGetandGetAll>>(_userPermissionReadRepository.GetAll(false)).Select(rp => rp.UserAccess).OrderBy(UserAccess => UserAccess).ToList().SequenceEqual(GetUserPermissions(localTime).Select(rp => rp.UserAccess).OrderBy(username => username).ToList()) == false)
            {
                List<UserPermission> defaultPermissions = GetUserPermissions(localTime);
                await _userPermissionWriteRepository.AddRangeAsync(defaultPermissions);

                var userPermissionResult = await _userPermissionWriteRepository.SaveAsync();
                if (userPermissionResult == -1)
                {
                    throw new InvalidOperationException("Failed to create the UserPermissions.");
                }


                try
                {
                    var userClaims = _mapper.Map<List<UserPermissionDTOforGetandGetAll>>(_userPermissionReadRepository.GetAll(false)).ToList().Select(permission => new UserClaimDTOforGetandGetAll
                    {
                        UserPermitionId = permission.Id,
                        UserId = userId,
                        CreatedDate = localTime
                    }).ToList();

                    await _userClaimWriteRepository.AddRangeAsync(_mapper.Map<List<UserClaim>>(userClaims));
                    var userClaimResult = await _userClaimWriteRepository.SaveAsync();
                    if (userClaimResult == -1)
                    {
                        throw new InvalidOperationException("Failed to create the UserClaims.");
                    }

                    #region NotificationCurrentUser


                    await NotificationCurrentUser(model.Username, "Post User", "Create User");

                    #endregion

                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("An error occurred while saving user claims.");
                }
            }
        }

        public class RolePermissionComparer : IEqualityComparer<RolePermission>
        {
            public bool Equals(RolePermission x, RolePermission y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode(RolePermission obj)
            {
                return obj.Id.GetHashCode();
            }
        }

        public async Task<GetUserDTOModel> GetUserById(Guid Id, ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.Identity.IsAuthenticated)
            {
                if (!_mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false)).AsEnumerable().Any(i => string.IsNullOrEmpty(i.RefreshToken) && i.Username == claimsPrincipal.Identity.Name))
                {

                    var users = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false));
                    var roles = _mapper.Map<List<RoleDTOforGetandGetAll>>(_roleReadRepository.GetAll(false));
                    var userPermissions = _mapper.Map<List<UserPermissionDTOforGetandGetAll>>(_userPermissionReadRepository.GetAll(false));
                    var userRoles = _mapper.Map<List<UserRoleDTOforGetandGetAll>>(_userRoleReadRepository.GetAll(false));
                    var rolePermissions = _mapper.Map<List<RolePermissionDTOforGetandGetAll>>(_rolePermissionReadRepository.GetAll(false)).Distinct().ToList();
                    var roleClaims = _mapper.Map<List<RoleClaimDTOforGetandGetAll>>(_roleClaimReadRepository.GetAll(false)).Distinct().ToList();
                    var userClaims = _mapper.Map<List<UserClaimDTOforGetandGetAll>>(_userClaimReadRepository.GetAll(false));

                    var currentUserDTO = users.FirstOrDefault(u => u.Id == Id);
                    if (currentUserDTO == null)
                    {
                        throw new NotFoundException("User not found.");
                    }

                    var permission = new PermissionDTO
                    {
                        UserPermissions = userPermissions.ToList(),
                        RolePermissions = rolePermissions.Where(rp => roleClaims.Any(rc => rc.RolePermissionId == rp.Id && userRoles.Any(ur => ur.RoleId == rc.RoleId && ur.UserId == currentUserDTO.Id))).ToList(),
                        Roles = roles.Where(r => userRoles.Any(ur => ur.RoleId == r.Id && ur.UserId == currentUserDTO.Id)).ToList()
                    };

                    var authResult = new GetUserDTOModel
                    {
                        Id = currentUserDTO.Id.ToString(),
                        Username = currentUserDTO.Username,
                        Name = currentUserDTO.Name,
                        Surname = currentUserDTO.Surname,
                        Email = currentUserDTO.Email,
                        Password = currentUserDTO.Password,
                        ConfirmPassword = currentUserDTO.ConfirmPassword,
                        Birthday = currentUserDTO.Birthday,
                        CreatedDate = currentUserDTO.CreatedDate?.ToString(),
                        UpdatedDate = currentUserDTO.UpdatedDate?.ToString(),
                        IsBlcok = currentUserDTO.IsBlcok,
                        IsActive = currentUserDTO.IsActive,
                        ProfileImage = currentUserDTO.ProfileImage,
                        Permitions = new List<PermissionDTO> { permission }
                    };

                    var cachedUser = await _cacheServiceGetAuthDTOModel.GetUser(authResult.Username);
                    if (cachedUser == null)
                    {

                        var GetAuthDTOModel = _mapper.Map<GetUserDTOModel>(authResult);
                        _cacheServiceGetAuthDTOModel.AddUser(authResult.Username, GetAuthDTOModel);
                    }


                    #region NotificationCurrentUser


                    await NotificationCurrentUser(claimsPrincipal.Identity.Name, "Get UserById", "View UserById");

                    #endregion

                    return await _cacheServiceGetAuthDTOModel.GetUser(authResult.Username);
                }
                else
                {
                    throw new UnauthorizedException("Current user is not authenticated.");
                }
            }
            else
            {
                throw new UnauthorizedException("Current user is not authenticated.");
            }
        }

        public async Task UpdateProfilePassword(UpdatePasswordDTO model, ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.Identity.IsAuthenticated)
            {

                if (!_mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false)).AsEnumerable().Any(i => string.IsNullOrEmpty(i.RefreshToken) && i.Username == claimsPrincipal.Identity.Name))
                {

                    var user = _userReadRepository.GetAll(false).FirstOrDefault(u => u.Username == claimsPrincipal.Identity.Name && u.Id == model.Id);


                    if (user == null)
                    {
                        throw new NotFoundException("User not found.");
                    }


                    if (await _cacheServiceUserDTOforGetandGetAll.GetProfile(user.Username) == null)
                    {

                        var userDTOforGetandGetAll = _mapper.Map<UserDTOforGetandGetAll>(user);
                        await _cacheServiceUserDTOforGetandGetAll.AddProfile(user.Username, userDTOforGetandGetAll);
                    }

                    var cachedUser = await _cacheServiceUserDTOforGetandGetAll.GetProfile(user.Username);

                    if (cachedUser.Password != PasswordComputeHash(model.OldPassword, Environment.GetEnvironmentVariable("Salt")))
                    {
                        throw new ConflictException("Enter the old password correctly.");
                    }

                    if (model.OldPassword == model.NewPassword)
                    {
                        throw new ConflictException("The new password cannot be the same as the old password.");
                    }




                    string oldImageUrl = cachedUser.ProfileImage;


                    var authClaims = new List<Claim>
                    {
                       new Claim(ClaimTypes.Name,  cachedUser.Username),
                       new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

                    var token = CreateToken(authClaims);
                    var refreshToken = GenerateRefreshToken();
                    int.TryParse(_configuration["JWT:RefreshTokenValidityInHours"], out int refreshTokenValidityInHours);

                    System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();

                    TimeZone localZone = TimeZone.CurrentTimeZone;
                    DateTime localTime = localZone.ToLocalTime(DateTime.UtcNow);



                    var userToUpdate = _mapper.Map<UserDTOforUpdate>(new UserDTOforUpdate
                    {
                        Id = model.Id,
                        Username = cachedUser.Username,
                        Name = cachedUser.Name,
                        Surname = cachedUser.Surname,
                        Email = cachedUser.Email,
                        Password = PasswordComputeHash(model.NewPassword, Environment.GetEnvironmentVariable("Salt")),
                        ConfirmPassword = PasswordComputeHash(model.NewPassword, Environment.GetEnvironmentVariable("Salt")),
                        Birthday = cachedUser.Birthday,
                        CreatedDate = cachedUser.CreatedDate,
                        UpdatedDate = localTime,
                        IsBlcok = false,
                        IsActive = false,
                        ProfileImage = cachedUser.ProfileImage,
                        RefreshToken = string.Empty,
                        RefreshTokenExpiryTime = null,
                        SecretKey = await GenerateTotpSecretKey(),
                    });

                    _userWriteRepository.Update(_mapper.Map<CraneFileManager.Domain.Entities.IdentityAuth.User>(userToUpdate));
                    var userUpdateResult = await _userWriteRepository.SaveAsync();

                    if (userUpdateResult == -1)
                    {
                        throw new InvalidOperationException("Failed to update the User password.");
                    }


                    await _cacheServiceUserDTOforUpdate.UpdateUser(cachedUser.Username, userToUpdate);
                    await _cacheServiceUserDTOforGetandGetAll.GetProfile(user.Username);

                }
            }
        }

        public async Task UpdateProfile(UpdateProfileDTO model, ClaimsPrincipal claimsPrincipal, string connectionStringAzure)
        {
            if (claimsPrincipal.Identity.IsAuthenticated)
            {

                if (!_mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false)).AsEnumerable().Any(i => string.IsNullOrEmpty(i.RefreshToken) && i.Username == claimsPrincipal.Identity.Name))
                {

                    var user = _userReadRepository.GetAll(false).FirstOrDefault(u => u.Username == claimsPrincipal.Identity.Name && u.Id == model.Id);


                    if (user == null)
                    {
                        throw new NotFoundException("User not found.");
                    }



                    if (await _cacheServiceUserDTOforGetandGetAll.GetProfile(user.Username) == null)
                    {

                        var userDTOforGetandGetAll = _mapper.Map<UserDTOforGetandGetAll>(user);
                        await _cacheServiceUserDTOforGetandGetAll.AddProfile(user.Username, userDTOforGetandGetAll);
                    }

                    var cachedUser = await _cacheServiceUserDTOforGetandGetAll.GetProfile(user.Username);


                    if (cachedUser.Password == PasswordComputeHash(model.Password, Environment.GetEnvironmentVariable("Salt")))
                    {
                        throw new ConflictException("The new password cannot be the same as the old password.");
                    }

                    if (model.ConfirmPassword != model.Password)
                    {
                        throw new ConflictException("Confirm Password does not match.");
                    }




                    string oldImageUrl = cachedUser.ProfileImage;


                    var authClaims = new List<Claim>
                    {
                       new Claim(ClaimTypes.Name,  cachedUser.Username),
                       new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

                    var token = CreateToken(authClaims);
                    var refreshToken = GenerateRefreshToken();
                    int.TryParse(_configuration["JWT:RefreshTokenValidityInHours"], out int refreshTokenValidityInHours);

                    System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();

                    TimeZone localZone = TimeZone.CurrentTimeZone;
                    DateTime localTime = localZone.ToLocalTime(DateTime.UtcNow);


                    string connectionString = GetAzureConnectionString(connectionStringAzure);
                    string containerName = "profile-images";
                    string userFolder = $"{model.Username}/";
                    string newBlobName = $"{userFolder}{model.Username}_{Guid.NewGuid()}{Path.GetExtension(model.ProfileImage.FileName)}";

                    var blobHttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = GetContentType(Path.GetExtension(model.ProfileImage.FileName)),
                        ContentDisposition = "inline"
                    };

                    var blobServiceClient = new BlobServiceClient(connectionString);
                    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                    await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

                    if (!string.IsNullOrEmpty(oldImageUrl))
                    {
                        var oldBlobName = Path.GetFileName(oldImageUrl);
                        var oldBlobClient = containerClient.GetBlobClient(oldBlobName);
                        await oldBlobClient.DeleteIfExistsAsync();
                    }

                    var newBlobClient = containerClient.GetBlobClient(newBlobName);
                    using (var stream = model.ProfileImage.OpenReadStream())
                    {
                        await newBlobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
                    }

                    string newImageUrl = newBlobClient.Uri.ToString();

                    var userToUpdate = _mapper.Map<UserDTOforUpdate>(new UserDTOforUpdate
                    {
                        Id = model.Id,
                        Username = model.Username,
                        Name = model.Name,
                        Surname = model.Surname,
                        Email = model.Email,
                        Password = PasswordComputeHash(model.Password, Environment.GetEnvironmentVariable("Salt")),
                        ConfirmPassword = PasswordComputeHash(model.ConfirmPassword, Environment.GetEnvironmentVariable("Salt")),
                        Birthday = model.Birthday,
                        CreatedDate = model.CreatedDate,
                        UpdatedDate = localTime,
                        IsBlcok = false,
                        IsActive = false,
                        ProfileImage = newImageUrl,
                        RefreshToken = string.Empty,
                        RefreshTokenExpiryTime = null,
                        SecretKey = await GenerateTotpSecretKey(),
                    });

                    _userWriteRepository.Update(_mapper.Map<CraneFileManager.Domain.Entities.IdentityAuth.User>(userToUpdate));
                    await _userWriteRepository.SaveAsync();

                    var userUpdateResult = await _userWriteRepository.SaveAsync();

                    if (userUpdateResult == -1)
                    {
                        throw new InvalidOperationException("Failed to update the User.");
                    }

                    await _cacheServiceUserDTOforUpdate.UpdateUser(cachedUser.Username, userToUpdate);
                    await _cacheServiceUserDTOforGetandGetAll.GetProfile(user.Username);

                }
                else
                {
                    throw new UnauthorizedException("Current user is not authenticated.");
                }
            }
            else
            {
                throw new UnauthorizedException("Current user is not authenticated.");
            }

        }


        public async Task UpdateUserBlock(UpdateUserBlockStatusDTO model, ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.Identity.IsAuthenticated)
            {
                if (!_mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false))
                    .AsEnumerable().Any(i => string.IsNullOrEmpty(i.RefreshToken) && i.Username == claimsPrincipal.Identity.Name))
                {
                    var users = _userReadRepository.GetAll(false).ToList();
                    var user = users.FirstOrDefault(u => u.Id == model.Id);
               

                    var roles = _mapper.Map<List<RoleDTOforGetandGetAll>>(_roleReadRepository.GetAll(false));
                    var adminRole = roles.FirstOrDefault(i=>i.Name=="Admin");


                    var userPermissions = _mapper.Map<List<UserPermissionDTOforGetandGetAll>>(_userPermissionReadRepository.GetAll(false));
                    var userRoles = _mapper.Map<List<UserRoleDTOforGetandGetAll>>(_userRoleReadRepository.GetAll(false));
                    var rolePermissions = _mapper.Map<List<RolePermissionDTOforGetandGetAll>>(_rolePermissionReadRepository.GetAll(false)).Distinct().ToList();
                    var roleClaims = _mapper.Map<List<RoleClaimDTOforGetandGetAll>>(_roleClaimReadRepository.GetAll(false)).Distinct().ToList();

                    var allUserDTOModels = new List<GetUserDTOModel>();





                    if (user == null)
                    {
                        throw new NotFoundException("User not found.");
                    }

                    if (users.Count <= 0)
                    {
                        throw new NotFoundException("User list is empty.");
                    }

                    var isAndminUserRole = userRoles.Any(ur => ur.UserId == user.Id && ur.RoleId == adminRole.Id);





                    if (isAndminUserRole)
                    {
                        throw new ForbiddenException("A user with the admin role cannot be blocked.");
                    }

                    else
                    {



                        if (users.Any(i => i.Id == model.Id))
                        {
                            if (users.Any(i => i.Id == model.Id && i.IsBlcok != model.IsBlcok))
                            {
                                var cachedUser = users.FirstOrDefault(i => i.Id == model.Id && i.IsBlcok != model.IsBlcok);

                                System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();
                                TimeZone localZone = TimeZone.CurrentTimeZone;
                                DateTime localTime = localZone.ToLocalTime(DateTime.UtcNow);

                                var userToUpdate = _mapper.Map<UserDTOforUpdate>(new UserDTOforUpdate
                                {
                                    Id = cachedUser.Id,
                                    Username = cachedUser.Username,
                                    Name = cachedUser.Name,
                                    Surname = cachedUser.Surname,
                                    Email = cachedUser.Email,
                                    Password = cachedUser.Password,
                                    ConfirmPassword = cachedUser.ConfirmPassword,
                                    Birthday = cachedUser.Birthday,
                                    CreatedDate = cachedUser.CreatedDate,
                                    UpdatedDate = localTime,
                                    IsBlcok = model.IsBlcok,
                                    IsActive = false,
                                    ProfileImage = string.Empty,
                                    RefreshToken = string.Empty,
                                    RefreshTokenExpiryTime = null,
                                    SecretKey = cachedUser.SecretKey,
                                });

                                _userWriteRepository.Update(_mapper.Map<CraneFileManager.Domain.Entities.IdentityAuth.User>(userToUpdate));
                                await _userWriteRepository.SaveAsync();

                                var userUpdateResult = await _userWriteRepository.SaveAsync();

                                if (userUpdateResult == -1)
                                {
                                    throw new InvalidOperationException("Failed to update the User.");
                                }




                                foreach (var item in users)
                                {
                                    var userRolesForCurrentUser = userRoles.Where(ur => ur.UserId == user.Id).ToList();

                                    var permission = new PermissionDTO
                                    {
                                        UserPermissions = userPermissions.ToList(),
                                        RolePermissions = rolePermissions.Where(rp => roleClaims.Any(rc => rc.RolePermissionId == rp.Id &&
                                            userRolesForCurrentUser.Any(ur => ur.RoleId == rc.RoleId))).ToList(),
                                        Roles = roles.Where(r => userRolesForCurrentUser.Any(ur => ur.RoleId == r.Id)).ToList()
                                    };

                                    var userDTO = new GetUserDTOModel
                                    {
                                        Id = item.Id.ToString(),
                                        Username = item.Username,
                                        Name = item.Name,
                                        Surname = item.Surname,
                                        Email = item.Email,
                                        Password = item.Password,
                                        ConfirmPassword = item.ConfirmPassword,
                                        Birthday = item.Birthday,
                                        CreatedDate = item.CreatedDate?.ToString(),
                                        UpdatedDate = item.UpdatedDate?.ToString(),
                                        IsBlcok = model.IsBlcok,
                                        IsActive = item.IsActive,
                                        ProfileImage = item.ProfileImage,
                                        Permitions = new List<PermissionDTO> { permission }
                                    };

                                    allUserDTOModels.Add(userDTO);
                                }


                                await _cacheServiceGetAuthDTOModel.UpdateUser(allUserDTOModels.FirstOrDefault(i => i.Id == model.Id.ToString()).Username, allUserDTOModels.FirstOrDefault(i => i.Id == model.Id.ToString()));


                                await Task.Delay(200);
                                await _cacheServiceUserDTOforGetandGetAll.GetAllUsers();
                            }
                            else
                            {
                                throw new ConflictException("Since the change made is identical to the existing change, the same choice cannot be made.");
                            }
                        }
                        else
                        {
                            throw new NotFoundException("User not found.");
                        }

                    }
                }
                else
                {
                    throw new UnauthorizedException("Current user is not authenticated.");
                }
            }
            else
            {
                throw new UnauthorizedException("Current user is not authenticated.");
            }
        }


        public async Task DeleteProfile(Guid Id, ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.Identity.IsAuthenticated)
            {
                if (!_mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false)).AsEnumerable().Any(i => string.IsNullOrEmpty(i.RefreshToken) && i.Username == claimsPrincipal.Identity.Name))
                {


                    var currentUser = claimsPrincipal.Identity.Name;

                    var users = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false));
                    var roles = _mapper.Map<List<RoleDTOforGetandGetAll>>(_roleReadRepository.GetAll(false));
                    var userPermissions = _mapper.Map<List<UserPermissionDTOforGetandGetAll>>(_userPermissionReadRepository.GetAll(false));
                    var userRoles = _mapper.Map<List<UserRoleDTOforGetandGetAll>>(_userRoleReadRepository.GetAll(false));
                    var rolePermissions = _mapper.Map<List<RolePermissionDTOforGetandGetAll>>(_rolePermissionReadRepository.GetAll(false)).Distinct().ToList();
                    var roleClaims = _mapper.Map<List<RoleClaimDTOforGetandGetAll>>(_roleClaimReadRepository.GetAll(false)).Distinct().ToList();
                    var userClaims = _mapper.Map<List<UserClaimDTOforGetandGetAll>>(_userClaimReadRepository.GetAll(false));

                    var currentUserDTO = users.FirstOrDefault(u => u.Username == currentUser);
                    if (currentUserDTO == null)
                    {
                        throw new NotFoundException("User not found.");
                    }

                    var permission = new PermissionDTO
                    {
                        UserPermissions = userPermissions.ToList(),
                        RolePermissions = rolePermissions.Where(rp => roleClaims.Any(rc => rc.RolePermissionId == rp.Id && userRoles.Any(ur => ur.RoleId == rc.RoleId && ur.UserId == currentUserDTO.Id))).ToList(),
                        Roles = roles.Where(r => userRoles.Any(ur => ur.RoleId == r.Id && ur.UserId == currentUserDTO.Id)).ToList()
                    };

                    var authResult = new GetUserDTOModel
                    {
                        Id = currentUserDTO.Id.ToString(),
                        Username = currentUserDTO.Username,
                        Name = currentUserDTO.Name,
                        Surname = currentUserDTO.Surname,
                        Email = currentUserDTO.Email,
                        Password = currentUserDTO.Password,
                        ConfirmPassword = currentUserDTO.ConfirmPassword,
                        Birthday = currentUserDTO.Birthday,
                        CreatedDate = currentUserDTO.CreatedDate?.ToString(),
                        UpdatedDate = currentUserDTO.UpdatedDate?.ToString(),
                        IsBlcok = currentUserDTO.IsBlcok,
                        IsActive = currentUserDTO.IsActive,
                        ProfileImage = currentUserDTO.ProfileImage,
                        Permitions = new List<PermissionDTO> { permission }
                    };



                    var user = await _userReadRepository.GetByIdAsync(Id);


                    user.IsActive = false;
                    user.RefreshToken = null;
                    user.RefreshTokenExpiryTime = null;

                    if (_userRoleReadRepository.GetAll(false).Any(I => I.UserId == user.Id && I.RoleId == _roleReadRepository.GetAll(false).Where(i => i.Name == "Admin").FirstOrDefault().Id))
                    {


                        throw new ForbiddenException($"Admin cannot be deleted");
                    }

                    if (user.Id == new Guid(authResult.Id))
                    {



                        var cachedUser = await _cacheServiceGetAuthDTOModel.GetProfile(authResult.Username);
                        if (cachedUser == null)
                        {
                            var GetAuthDTOModel = _mapper.Map<GetUserDTOModel>(authResult);
                            await _cacheServiceGetAuthDTOModel.AddProfile(authResult.Username, GetAuthDTOModel);
                        }



                        try
                        {


                            _userNotificationWriteRepository.RemoveRange(_userNotificationReadRepository.GetAll(false).Where(un => un.UserId == user.Id).ToList());


                            var userNotificationDeleteResult = await _userNotificationWriteRepository.SaveAsync();

                            if (userNotificationDeleteResult == -1)
                            {
                                throw new InvalidOperationException("Failed to delete the user Notification.");
                            }



                            var notificationId = _userNotificationReadRepository.GetAll(false)
                                .Where(un => un.UserId == user.Id)
                                .Select(un => un.NotificationId)
                                .FirstOrDefault();

                            if (notificationId != Guid.Empty)
                            {
                                _notificationWriteRepository.RemoveRange(_notificationReadRepository.GetAll(false)
                                    .Where(n => n.Id == notificationId).ToList());


                                var notificationDeleteResult = await _notificationWriteRepository.SaveAsync();




                                if (notificationDeleteResult == -1)
                                {
                                    throw new InvalidOperationException("Failed to delete the Notification.");
                                }

                            }

                            _userRoleWriteRepository.RemoveRange(_userRoleReadRepository.GetAll(false).Where(ur => ur.UserId == user.Id).ToList());


                            var userRoleDeleteResult = await _userRoleWriteRepository.SaveAsync();




                            if (userRoleDeleteResult == -1)
                            {
                                throw new InvalidOperationException("Failed to delete the Notification.");
                            }




                            _userClaimWriteRepository.RemoveRange(_userClaimReadRepository.GetAll(false).Where(ur => ur.UserId == user.Id).ToList());


                            var userClaimDeleteResult = await _userClaimWriteRepository.SaveAsync();


                            if (userClaimDeleteResult == -1)
                            {
                                throw new InvalidOperationException("Failed to delete the Notification.");
                            }


                            //_userFileWriteRepository.RemoveRange(_userFileReadRepository.GetAll(false).Where(ur => ur.UserId == user.Id).ToList());


                            //var userFileDeleteResult = await _userFileWriteRepository.SaveAsync();


                            //if (userFileDeleteResult == -1)
                            //{
                            //    throw new InvalidOperationException("Failed to delete the Notification.");
                            //}


                            //_userFolderWriteRepository.RemoveRange(_userFolderReadRepository.GetAll(false).Where(ur => ur.UserId == user.Id).ToList());


                            //var userFolderDeleteResult = await _userFolderWriteRepository.SaveAsync();


                            //if (userFolderDeleteResult == -1)
                            //{
                            //    throw new InvalidOperationException("Failed to delete the Notification.");
                            //}


                            //_userStorageWriteRepository.RemoveRange(_userStorageReadRepository.GetAll(false).Where(ur => ur.UserId == user.Id).ToList());


                            //var userStorageDeleteResult = await _userStorageWriteRepository.SaveAsync();


                            //if (userStorageDeleteResult == -1)
                            //{
                            //    throw new InvalidOperationException("Failed to delete the Notification.");
                            //}



                            await _userWriteRepository.RemoveByIdAsync(new Guid(authResult.Id));
                            var userDeleteResult = await _userWriteRepository.SaveAsync();


                            if (userDeleteResult == -1)
                            {
                                throw new InvalidOperationException("Failed to delete the User.");
                            }

                            await _cacheServiceUserDTOforUpdate.DeleteUser(authResult.Username);
                            await _cacheServiceUserDTOforGetandGetAll.GetAllUsers();



                        }
                        catch (DbUpdateException ex)
                        {
                            Console.Error.WriteLine($"Database update error: {ex.InnerException?.Message}");
                            throw new InvalidOperationException(ex.Message);
                        }

                        #region NotificationNoCurrentUser

                        System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();

                        TimeZone localZone = TimeZone.CurrentTimeZone;
                        DateTime localTime = localZone.ToLocalTime(DateTime.UtcNow);

                        await NotificationNoCurrentUser(localTime, "Delete Profile", "Delete Profile");

                        #endregion
                    }
                    else
                    {
                        throw new ForbiddenException($"Enter the profile id correctly");
                    }
                }
                else
                {
                    throw new UnauthorizedException("Current user is not authenticated.");
                }
            }
            else
            {
                throw new UnauthorizedException("Current user is not authenticated.");
            }
        }

        public async Task DeleteUser(Guid Id, ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.Identity.IsAuthenticated)
            {
                if (!_mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false)).AsEnumerable().Any(i => string.IsNullOrEmpty(i.RefreshToken) && i.Username == claimsPrincipal.Identity.Name))
                {


                    var currentUser = claimsPrincipal.Identity.Name;

                    var users = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false));
                    var roles = _mapper.Map<List<RoleDTOforGetandGetAll>>(_roleReadRepository.GetAll(false));
                    var userPermissions = _mapper.Map<List<UserPermissionDTOforGetandGetAll>>(_userPermissionReadRepository.GetAll(false));
                    var userRoles = _mapper.Map<List<UserRoleDTOforGetandGetAll>>(_userRoleReadRepository.GetAll(false));
                    var rolePermissions = _mapper.Map<List<RolePermissionDTOforGetandGetAll>>(_rolePermissionReadRepository.GetAll(false)).Distinct().ToList();
                    var roleClaims = _mapper.Map<List<RoleClaimDTOforGetandGetAll>>(_roleClaimReadRepository.GetAll(false)).Distinct().ToList();
                    var userClaims = _mapper.Map<List<UserClaimDTOforGetandGetAll>>(_userClaimReadRepository.GetAll(false));

                    var currentUserDTO = users.FirstOrDefault(u => u.Username == currentUser);
                    if (currentUserDTO == null)
                    {
                        throw new NotFoundException("User not found.");
                    }

                    var permission = new PermissionDTO
                    {
                        UserPermissions = userPermissions.ToList(),
                        RolePermissions = rolePermissions.Where(rp => roleClaims.Any(rc => rc.RolePermissionId == rp.Id && userRoles.Any(ur => ur.RoleId == rc.RoleId && ur.UserId == currentUserDTO.Id))).ToList(),
                        Roles = roles.Where(r => userRoles.Any(ur => ur.RoleId == r.Id && ur.UserId == currentUserDTO.Id)).ToList()
                    };





                    var user = await _userReadRepository.GetByIdAsync(Id);


                    user.IsActive = false;
                    user.RefreshToken = null;
                    user.RefreshTokenExpiryTime = null;

                    if (_userRoleReadRepository.GetAll(false).Any(I => I.UserId == user.Id && I.RoleId == _roleReadRepository.GetAll(false).Where(i => i.Name == "Admin").FirstOrDefault().Id))
                    {


                        throw new ForbiddenException($"Admin cannot be deleted");
                    }









                    try
                    {


                        _userNotificationWriteRepository.RemoveRange(_userNotificationReadRepository.GetAll(false).Where(un => un.UserId == user.Id).ToList());


                        var userNotificationDeleteResult = await _userNotificationWriteRepository.SaveAsync();

                        if (userNotificationDeleteResult == -1)
                        {
                            throw new InvalidOperationException("Failed to delete the user Notification.");
                        }



                        var notificationId = _userNotificationReadRepository.GetAll(false).Where(un => un.UserId == user.Id).Select(un => un.NotificationId).FirstOrDefault();

                        if (notificationId != Guid.Empty)
                        {
                            _notificationWriteRepository.RemoveRange(_notificationReadRepository.GetAll(false)
                                .Where(n => n.Id == notificationId).ToList());


                            var notificationDeleteResult = await _notificationWriteRepository.SaveAsync();




                            if (notificationDeleteResult == -1)
                            {
                                throw new InvalidOperationException("Failed to delete the Notification.");
                            }

                        }

                        _userRoleWriteRepository.RemoveRange(_userRoleReadRepository.GetAll(false).Where(ur => ur.UserId == user.Id).ToList());


                        var userRoleDeleteResult = await _userRoleWriteRepository.SaveAsync();




                        if (userRoleDeleteResult == -1)
                        {
                            throw new InvalidOperationException("Failed to delete the Notification.");
                        }




                        _userClaimWriteRepository.RemoveRange(_userClaimReadRepository.GetAll(false).Where(ur => ur.UserId == user.Id).ToList());


                        var userClaimDeleteResult = await _userClaimWriteRepository.SaveAsync();


                        if (userClaimDeleteResult == -1)
                        {
                            throw new InvalidOperationException("Failed to delete the Notification.");
                        }


                        //_userFileWriteRepository.RemoveRange(_userFileReadRepository.GetAll(false).Where(ur => ur.UserId == user.Id).ToList());


                        //var userFileDeleteResult = await _userFileWriteRepository.SaveAsync();


                        //if (userFileDeleteResult == -1)
                        //{
                        //    throw new InvalidOperationException("Failed to delete the Notification.");
                        //}


                        //_userFolderWriteRepository.RemoveRange(_userFolderReadRepository.GetAll(false).Where(ur => ur.UserId == user.Id).ToList());


                        //var userFolderDeleteResult = await _userFolderWriteRepository.SaveAsync();


                        //if (userFolderDeleteResult == -1)
                        //{
                        //    throw new InvalidOperationException("Failed to delete the Notification.");
                        //}


                        //_userStorageWriteRepository.RemoveRange(_userStorageReadRepository.GetAll(false).Where(ur => ur.UserId == user.Id).ToList());


                        //var userStorageDeleteResult = await _userStorageWriteRepository.SaveAsync();


                        //if (userStorageDeleteResult == -1)
                        //{
                        //    throw new InvalidOperationException("Failed to delete the Notification.");
                        //}



                        await _userWriteRepository.RemoveByIdAsync(user.Id);
                        var userDeleteResult = await _userWriteRepository.SaveAsync();


                        if (userDeleteResult == -1)
                        {
                            throw new InvalidOperationException("Failed to delete the User.");
                        }

                        await _cacheServiceUserDTOforUpdate.DeleteUser(user.Username);
                        await _cacheServiceUserDTOforGetandGetAll.GetAllUsers();



                    }
                    catch (DbUpdateException ex)
                    {
                        Console.Error.WriteLine($"Database update error: {ex.InnerException?.Message}");
                        throw new InvalidOperationException(ex.Message);
                    }

                    #region NotificationCurrentUser



                    await NotificationCurrentUser(currentUser, "Delete User", "Delete User");

                    #endregion

                }
                else
                {
                    throw new UnauthorizedException("Current user is not authenticated.");
                }
            }
            else
            {
                throw new UnauthorizedException("Current user is not authenticated.");
            }
        }








        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {

                throw new SecurityTokenException("Invalid token");
            }

            return principal;

        }

        private static List<UserPermission> GetUserPermissions(DateTime localTime)
        {
            return new List<UserPermission>
            {
                    new UserPermission {Id=Guid.NewGuid(), UserAccess = "Create", UserAccessDescription = "Create Item", CreatedDate = localTime },
                    new UserPermission {Id=Guid.NewGuid(),UserAccess = "Read", UserAccessDescription = "Read Item", CreatedDate = localTime },
                    new UserPermission {Id = Guid.NewGuid(),  UserAccess = "Update", UserAccessDescription = "Update Item", CreatedDate = localTime },
                    new UserPermission {Id = Guid.NewGuid(),  UserAccess = "Delete", UserAccessDescription = "Delete Item", CreatedDate = localTime }
            };
        }

        public List<RolePermissionDTOforGetandGetAll> GetRolePermissionsForAdmin(DateTime localTime)
        {
            return new List<RolePermissionDTOforGetandGetAll>
            {
                new RolePermissionDTOforGetandGetAll {
                   Id=Guid.NewGuid(),
                   Method = "Get_NotificationsForAdmin",
                   MethodDescription = "View Notifications",
                   CreatedDate = localTime
                },



                new RolePermissionDTOforGetandGetAll {
                   Id=Guid.NewGuid(),
                   Method = "Get_ProfileForAdmin",
                   MethodDescription = "View Profile",
                   CreatedDate = localTime
                },

                new RolePermissionDTOforGetandGetAll {
                   Id=Guid.NewGuid(),
                   Method = "Get_MesssagesForAdmin",
                   MethodDescription = "View Messages",
                   CreatedDate = localTime
                },

                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Get_UsersForAdmin",
                   MethodDescription = "View Users",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Put_UserForAdmin",
                   MethodDescription = "Update User",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Put_ProfileForAdmin",
                   MethodDescription = "Update Profile",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Put_PasswordForAdmin",
                   MethodDescription = "Update Password",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Get_UserByIdForAdmin",
                   MethodDescription = "View UserById",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Delete_ProfileForAdmin",
                   MethodDescription = "Remove Profile",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Delete_UserForAdmin",
                   MethodDescription = "Remove User",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Post_RefreshTokenForAdmin",
                   MethodDescription = "Add RefreshToken",
                   CreatedDate = localTime
                },                
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Put_UserBlockForAdmin",
                   MethodDescription = "Update UserBlock",
                   CreatedDate = localTime
                },


                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Post_FileForAdmin",
                   MethodDescription = "Add File",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Delete_FileForAdmin",
                   MethodDescription = "Remove File",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Get_FilesForAdmin",
                   MethodDescription = "View Files",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Get_FileByIdForAdmin",
                   MethodDescription = "View File",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                   Id=Guid.NewGuid(),
                   Method = "Put_FileForAdmin",
                   MethodDescription = "Update File",
                   CreatedDate = localTime
                },


                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Post_FileShareForAdmin",
                   MethodDescription = "Add FileShare",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Delete_FileShareForAdmin",
                   MethodDescription = "Remove FileShare",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Get_FileSharesForAdmin",
                   MethodDescription = "View FileShares",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Get_FileShareByIdForAdmin",
                   MethodDescription = "View FileShare",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                   Id=Guid.NewGuid(),
                   Method = "Put_FileShareForAdmin",
                   MethodDescription = "Update FileShare",
                   CreatedDate = localTime
                },

                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Post_FileTrashCanForAdmin",
                   MethodDescription = "Add FileTrashCan",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                   Id=Guid.NewGuid(),
                   Method = "Put_FileTrashCanForAdmin",
                   MethodDescription = "Update FileTrashCan",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Delete_FileTrashCanForAdmin",
                   MethodDescription = "Remove FileTrashCan",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                   Id=Guid.NewGuid(),
                   Method = "Get_FileTrashCanForAdmin",
                   MethodDescription = "View FileTrashCan",
                   CreatedDate = localTime
                },
            };
        }

        private List<RolePermissionDTOforGetandGetAll> GetRolePermissionsForUser(DateTime localTime)
        {
            return new List<RolePermissionDTOforGetandGetAll>
            {
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Get_ProfileForUser",
                   MethodDescription = "View Profile",
                   CreatedDate = localTime
                },

                new RolePermissionDTOforGetandGetAll {
                   Id=Guid.NewGuid(),
                   Method = "Get_MesssagesForUser",
                   MethodDescription = "View Messages",
                   CreatedDate = localTime
                },

                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Put_PasswordForUser",
                   MethodDescription = "Update Password",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Put_ProfileForUser",
                   MethodDescription = "Update Profile",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Delete_ProfileForUser",
                   MethodDescription = "Remove Profile",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Post_RefreshTokenForUser",
                   MethodDescription = "Add RefreshToken",
                   CreatedDate = localTime
                },


                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Post_FileForUser",
                   MethodDescription = "Add File",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Delete_FileForUser",
                   MethodDescription = "Remove File",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Get_FilesForUser",
                   MethodDescription = "View Files",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Get_FileByIdForUser",
                   MethodDescription = "View File",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                   Id=Guid.NewGuid(),
                   Method = "Put_FileForUser",
                   MethodDescription = "Update File",
                   CreatedDate = localTime
                },


                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Post_FileShareForUser",
                   MethodDescription = "Add FileShare",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Delete_FileShareForUser",
                   MethodDescription = "Remove FileShare",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Get_FileSharesForUser",
                   MethodDescription = "View FileShares",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Get_FileShareByIdForUser",
                   MethodDescription = "View FileShare",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                   Id=Guid.NewGuid(),
                   Method = "Put_FileShareForUser",
                   MethodDescription = "Update FileShare",
                   CreatedDate = localTime
                },                
                
                
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Post_FileTrashCanForUser",
                   MethodDescription = "Add FileTrashCan",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                   Id=Guid.NewGuid(),
                   Method = "Put_FileTrashCanForUser",
                   MethodDescription = "Update FileTrashCan",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                      Id=Guid.NewGuid(),
                   Method = "Delete_FileTrashCanForUser",
                   MethodDescription = "Remove FileTrashCan",
                   CreatedDate = localTime
                },
                new RolePermissionDTOforGetandGetAll {
                   Id=Guid.NewGuid(),
                   Method = "Get_FileTrashCanForUser",
                   MethodDescription = "View FileTrashCan",
                   CreatedDate = localTime
                },
            };
        }

        private string GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }

        private string GetAzureConnectionString(string connectionStringAzure)
        {
            string azuriteConnectionString = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_AZURE_STORAGE_CONNECTION_STRING");
            return !string.IsNullOrEmpty(azuriteConnectionString) ? azuriteConnectionString : connectionStringAzure;
        }

        public static string PasswordComputeHash(string password, string pepper)
        {
            using var sha256 = SHA256.Create();
            var passwordSaltPepper = $"{password}{pepper}";
            var byteValue = Encoding.UTF8.GetBytes(passwordSaltPepper);
            var byteHash = sha256.ComputeHash(byteValue);
            return Convert.ToBase64String(byteHash);
        }

        private JwtSecurityToken CreateToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            _ = int.TryParse(_configuration["JWT:TokenValidityInHour"], out int TokenValidityInHour);


            System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();

            TimeZone localZone = TimeZone.CurrentTimeZone;
            DateTime localTime = localZone.ToLocalTime(DateTime.UtcNow);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidateIssuer"],
                audience: _configuration["JWT:ValidateAudience"],
                expires: localTime.AddHours(TokenValidityInHour),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[128];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private async Task NotificationNoCurrentUser(DateTime localTime, string title, string description)
        {
            var notification = new Domain.Entities.Models.Notification
            {
                Description = $"{description}",
                NotificationDate = localTime,
                Title = $"{title}"
            };


            try
            {
                System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();



                var users_ = await _userReadRepository.GetAll(false).ToListAsync();

                var currentUserEntity = new CraneFileManager.Domain.Entities.IdentityAuth.User();




                var notificationDto = new NotificationDTOforGetandGetAll
                {

                    Description = $"{notification.Description}",
                    NotificationDate = localTime,
                    Title = notification.Title
                };

                var notificationEntity = _mapper.Map<Notification>(notificationDto);
                await _notificationWriteRepository.AddAsync(notificationEntity);
                await _notificationWriteRepository.SaveAsync();






            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine($"Database update error: {ex.InnerException?.Message}");
                throw new InvalidOperationException(ex.Message);
            }
        }



        private async Task NotificationCurrentUser(string? currentUser, string title, string description)
        {
            System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();

            TimeZone localZone = TimeZone.CurrentTimeZone;
            DateTime localTime = localZone.ToLocalTime(DateTime.UtcNow);




            try
            {


                var users_ = await _userReadRepository.GetAll(false).ToListAsync();

                var currentUserEntity = new CraneFileManager.Domain.Entities.IdentityAuth.User();

                var userProfile = users_.FirstOrDefault(i => i.Username == currentUser);

                if (userProfile.Id != null)
                {
                    currentUserEntity = users_.FirstOrDefault(u => u.Id == userProfile.Id);


                    var notificationDto = new NotificationDTOforGetandGetAll
                    {

                        Description = $"{currentUserEntity.Username} sent a message: {description}",
                        NotificationDate = localTime,
                        Title = title
                    };

                    var notificationEntity = _mapper.Map<Notification>(notificationDto);
                    await _notificationWriteRepository.AddAsync(notificationEntity);
                    await _notificationWriteRepository.SaveAsync();

                    if (users_.Any(i => i.Id == userProfile.Id))
                    {
                        var userNotification = new UserNotification
                        {
                            NotificationId = notificationEntity.Id,
                            UserId = currentUserEntity.Id,
                            CreatedDate = localTime,
                            UpdatedDate = null,
                        };

                        await _userNotificationWriteRepository.AddAsync(userNotification);
                        await _userNotificationWriteRepository.SaveAsync();
                    }


                }
            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine($"Database update error: {ex.InnerException?.Message}");
                throw new InvalidOperationException(ex.Message);
            }
        }

    }
}
