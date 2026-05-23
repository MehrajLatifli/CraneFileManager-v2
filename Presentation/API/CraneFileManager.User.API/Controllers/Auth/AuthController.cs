using Asp.Versioning;
using AutoMapper;
using Azure.Core;
using CraneFileManager.Application.Mapper.DTO.AuthDTO;
using CraneFileManager.Application.Services.Abstract;
using CraneFileManager.Application.Validations;
using CraneFileManager.Domain.Entities.AuthModels;
using CraneFileManager.Domain.Entities.Configurations;
using CraneFileManager.Infrastructure.SignalR;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using CraneFileManager.Persistence.ServiceExtensions;
using CraneFileManager.User.API.API_Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Security.Claims;

namespace CraneFileManager.User.API.Controllers.Auth
{
    [ApiVersion(1, Deprecated = false)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        public readonly IMapper _mapper;
        private readonly IAuthService _authservice;
        private readonly INotificationService _notificationService;
        private readonly CraneFileManagerContext _craneFileManagerContext;
        private readonly NotificationHubService _notificationHubService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppSettings _appSettings;

        public AuthController(IMapper mapper, IAuthService authservice, CraneFileManagerContext craneFileManagerContext, NotificationHubService notificationHubService, INotificationService notificationService, IWebHostEnvironment webHostEnvironment, AppSettings appSettings)
        {
            _mapper = mapper;
            _authservice = authservice;
            _craneFileManagerContext = craneFileManagerContext;
            _notificationHubService = notificationHubService;
            _notificationService = notificationService;
            _webHostEnvironment = webHostEnvironment;
            _appSettings = appSettings;
        }

        [HttpPost]
        [Route(Routes.RegisterAdmin)]
        [Produces("application/json")]
        public async Task<IActionResult> RegisterAdmin([FromForm] RegisterDTO model)
        {

            await _authservice.RegisterAdmin(model, _appSettings.ConnectionAzureStorage);


            await _notificationHubService.StartAsync();

            if (_notificationHubService.IsConnected)
            {
                var notifications = await _notificationService.GetNotifications(User);
                var lastNotification = notifications.OrderBy(o => o.NotificationDate).Where(i => i.Title == "Register Admin").ToList().LastOrDefault();

                if (lastNotification != null)
                {
                    await _notificationHubService.SendMessageToUserAsync(null, lastNotification);
                }
            }



            return Ok(new Response { Status = "Success", Message = $"{model.Username} created successfully!" });
        }

        [HttpPost]
        [Route(Routes.RegisterUser)]
        [Produces("application/json")]
        public async Task<IActionResult> RegisterUser([FromForm] RegisterDTO model)
        {

            await _authservice.RegisterUser(model, _appSettings.ConnectionAzureStorage);


            await _notificationHubService.StartAsync();

            if (_notificationHubService.IsConnected)
            {
                var notifications = await _notificationService.GetNotifications(User);
                var lastNotification = notifications.OrderBy(o => o.NotificationDate).Where(i => i.Title == "Register User").ToList().LastOrDefault();

                if (lastNotification != null)
                {
                    await _notificationHubService.SendMessageToUserAsync(null, lastNotification);
                }
            }

            return Ok(new Response { Status = "Success", Message = $"{model.Username} created successfully!" });
        }

        [HttpPost]
        [Route(Routes.Login)]
        [Produces("application/json")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var loginResult = await _authservice.Login(model);

            await _notificationHubService.StartAsync();

            if (_notificationHubService.IsConnected)
            {
                var notifications = await _notificationService.GetNotifications(User);
                var lastNotification = notifications.OrderBy(o => o.NotificationDate).ToList().LastOrDefault();

                if (lastNotification != null)
                {
                    await _notificationHubService.SendMessageToUserAsync(null, lastNotification);
                }
            }

            return Ok(loginResult);
        }

        [HttpPost("login-2fa")]
        [Produces("application/json")]
        public async Task<IActionResult> LoginWith2FA([FromBody] LoginDTO2FA model)
        {
            var response = await _authservice.LoginWith2FA(model);
            return Ok(response);
        }

        [HttpGet("generate-2fa-qrcode")]
        public async Task<IActionResult> Generate2FAQRCode(string username)
        {
            string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "swagger-ui", "qrlogo3.png");

            var qrCodeImage = await _authservice.Generate2FAQRCode(username, logoPath);



            return File(qrCodeImage, "image/png");
        }







        [HttpGet("GenerateTotpCode")]
        [Produces("application/json")]
        public async Task<IActionResult> GenerateTotpCode(string username)
        {

            string digits = await _authservice.GenerateTotpCode(username);


            return Ok(new { digits = digits });
        }



        [HttpPost]
        [Route(Routes.Logout)]
        [Produces("application/json")]
        public async Task<IActionResult> Logout()
        {
            await _authservice.Logout(User);

            await _notificationHubService.StartAsync();

            if (_notificationHubService.IsConnected)
            {
                var notifications = await _notificationService.GetNotifications(User);
                var lastNotification = notifications.OrderBy(o => o.NotificationDate).Where(i => i.Title == "LogOut").ToList().LastOrDefault();

                if (lastNotification != null)
                {
                    await _notificationHubService.SendMessageToUserAsync(null, lastNotification);
                }
            }

            return NoContent();
        }

        [HttpPost]
        [Route(Routes.RefreshToken)]
        [Produces("application/json")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin, UserRoles.User }, CustomRolePermissions = new[] { "Post_RefreshTokenForAdmin", "Post_RefreshTokenForUser" }, CustomUserPermissions = new[] { "Create" })]
        public async Task<IActionResult> RefreshToken(TokenModel tokenModel)
        {

            return Ok(await _authservice.RefreshToken(tokenModel, User));

        }

        [HttpGet]
        [Route(Routes.Profile)]
        [Produces("application/json")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin, UserRoles.User }, CustomRolePermissions = new[] { "Get_ProfileForAdmin", "Get_ProfileForUser" }, CustomUserPermissions = new[] { "Read" })]
        [OutputCache(Duration = 10)]
        public async Task<IActionResult> Profile()
        {
            var userProfile = await _authservice.Profile(User);

            await _notificationHubService.StartAsync();

            if (_notificationHubService.IsConnected)
            {
                var notifications = await _notificationService.GetNotifications(User);
                var lastNotification = notifications.OrderBy(o => o.NotificationDate).Where(i => i.Title == "Get Profile").ToList().LastOrDefault();

                if (lastNotification != null)
                {
                    await _notificationHubService.SendMessageToUserAsync(userProfile.Id, lastNotification);
                }
            }

            return Ok(userProfile);
        }


        [HttpGet]
        [Route(Routes.Profile + " xml")]
        [Produces("application/xml")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin, UserRoles.User }, CustomRolePermissions = new[] { "Get_ProfileForAdmin", "Get_ProfileForUser" }, CustomUserPermissions = new[] { "Read" })]
        public async Task<IActionResult> Profile2()
        {
            return Ok(await _authservice.Profile(User));



        }

        [HttpGet]
        [Route(Routes.Profile + " yaml")]
        [Produces("application/yaml")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin, UserRoles.User }, CustomRolePermissions = new[] { "Get_ProfileForAdmin", "Get_ProfileForUser" }, CustomUserPermissions = new[] { "Read" })]
        public async Task<IActionResult> Profile3()
        {
            return Ok(await _authservice.Profile(User));



        }

        [HttpGet]
        [Route(Routes.Profile + " csv")]
        [Produces("application/csv")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin, UserRoles.User }, CustomRolePermissions = new[] { "Get_ProfileForAdmin", "Get_ProfileForUser" }, CustomUserPermissions = new[] { "Read" })]
        public async Task<IActionResult> Profile4()
        {
            var profile = await _authservice.Profile(User);
            return Ok(new List<GetUserDTOModel> { profile });



        }

        [HttpPut]
        [Route(Routes.Profile)]
        [Produces("application/json")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin, UserRoles.User }, CustomRolePermissions = new[] { "Put_ProfileForAdmin", "Put_ProfileForUser" }, CustomUserPermissions = new[] { "Update" })]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDTO model)
        {
            await _authservice.UpdateProfile(model, User, _appSettings.ConnectionAzureStorage);
            return Ok(new Response { Status = "Success", Message = $"{model.Username} updated successfully!" });


        }

        [HttpPut]
        [Route(Routes.ProfilePassword)]
        [Produces("application/json")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin, UserRoles.User }, CustomRolePermissions = new[] { "Put_PasswordForAdmin", "Put_PasswordForUser" }, CustomUserPermissions = new[] { "Update" })]
        public async Task<IActionResult> UpdateProfilePassword([FromForm] UpdatePasswordDTO model)
        {
            await _authservice.UpdateProfilePassword(model, User);
            return Ok(new Response { Status = "Success", Message = $"Old pasword updated successfully!" });


        }




        [HttpDelete]
        [Route(Routes.DeleteProfile)]
        [Produces("application/json")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin, UserRoles.User }, CustomRolePermissions = new[] { "Delete_ProfileForAdmin", "Delete_ProfileForUser" }, CustomUserPermissions = new[] { "Delete" })]
        public async Task<IActionResult> DeleteProfile(Guid id)
        {
            await _authservice.DeleteProfile(id, User);
            return Ok(new Response { Status = "Success", Message = $"Profile deleted successfully!" });


        }


        [HttpGet]
        [Route(Routes.User)]
        [Produces("application/json")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin }, CustomRolePermissions = new[] { "Get_UsersForAdmin" }, CustomUserPermissions = new[] { "Read" })]
        public async Task<IActionResult> ViewUsers()
        {


            if (_notificationHubService.IsConnected)
            {
                var notifications = await _notificationService.GetNotifications(User);
                var lastNotification = notifications.OrderBy(o => o.NotificationDate).Where(i => i.Title == "Get Users").ToList().LastOrDefault();

                if (lastNotification != null)
                {
                    foreach (var item in await _authservice.GetUsers(User))
                    {
                        await _notificationHubService.SendMessageToUserAsync(item.Id, lastNotification);
                    }

                }
            }


            return Ok(await _authservice.GetUsers(User));



        }

        [HttpGet]
        [Route(Routes.UserById)]
        [Produces("application/json")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin }, CustomRolePermissions = new[] { "Get_UserByIdForAdmin" }, CustomUserPermissions = new[] { "Read" })]
        public async Task<IActionResult> ViewUserByID(Guid id)
        {

            if (_notificationHubService.IsConnected)
            {
                var notifications = await _notificationService.GetNotifications(User);
                var lastNotification = notifications.OrderBy(o => o.NotificationDate).Where(i => i.Title == "Get UserById").ToList().LastOrDefault();

                if (lastNotification != null)
                {

                    await _notificationHubService.SendMessageToUserAsync(id.ToString(), lastNotification);


                }
            }


            return Ok(await _authservice.GetUserById(id, User));



        }

        [HttpPut]
        [Route(Routes.UserBlockStatus)]
        [Produces("application/json")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin }, CustomRolePermissions = new[] { "Put_UserBlockForAdmin" }, CustomUserPermissions = new[] { "Update" })]
        public async Task<IActionResult> UpdateProfilePassword([FromForm] UpdateUserBlockStatusDTO model)
        {
            await _authservice.UpdateUserBlock(model, User);
            return Ok(new Response { Status = "Success", Message = $"The block status of the user with id {model.Id} has been changed to {model.IsBlcok}." });


        }

        [HttpDelete]
        [Route(Routes.DeleteUser)]
        [Produces("application/json")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin }, CustomRolePermissions = new[] { "Delete_UserForAdmin" }, CustomUserPermissions = new[] { "Delete" })]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            await _authservice.DeleteUser(id, User);
            return Ok(new Response { Status = "Success", Message = $"User deleted successfully!" });


        }


    }
}
