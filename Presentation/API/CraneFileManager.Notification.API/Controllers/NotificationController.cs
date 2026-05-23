using Asp.Versioning;
using CraneFileManager.Application.Mapper.DTO.AuthDTO;
using CraneFileManager.Application.Mapper.DTO.NotificationDTO;
using CraneFileManager.Application.Services.Abstract;
using CraneFileManager.Application.Validations;
using CraneFileManager.Domain.Entities.AuthModels;
using CraneFileManager.Infrastructure.SignalR;
using CraneFileManager.Notification.API.BackgroundServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CraneFileManager.Notification.API.Controllers
{
    [ApiVersion(1, Deprecated = false)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationHubService _notificationHubService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IAuthService _authservice;

        public NotificationController(NotificationHubService notificationHubService, IHubContext<NotificationHub> hubContext, IAuthService authservice)
        {
            _notificationHubService = notificationHubService;
            _hubContext = hubContext;
            _authservice = authservice;
        }

        [HttpGet]
        [Route("check")]
        [Produces("application/json")]
        public IActionResult GetCheck()
        {
            // You can perform any necessary checks here (e.g., database connection).
            // For simplicity, we will just return a 200 OK response.

            return Ok(new { status = "Healthy" });
        }


        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] NotificationDTOforGetandGetAll message)
        {
            if (string.IsNullOrEmpty(message.Description))
            {
                return BadRequest("Description is required.");
            }

            await _hubContext.Clients.User(message.Description).SendAsync("ReceiveMessage", message);
            return Ok();
        }

        [Authorize]
        [HttpGet("messages")]
        [Produces("application/json")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin }, CustomRolePermissions = new[] { "Get_NotificationsForAdmin" }, CustomUserPermissions = new[] { "Read" })]
        public async Task<IActionResult> GetMessages(CancellationToken cancellationToken)
        {
            var channel = NotificationBackgroundService.Subscribe();
            var writer = channel.Writer;

            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            try
            {
                await foreach (var message in channel.Reader.ReadAllAsync(cancellationToken))
                {
                    var jsonMessage = JsonSerializer.Serialize(message);
                    await Response.WriteAsync($"data: {jsonMessage}\n\n");
                    await Response.Body.FlushAsync(cancellationToken);
                }
            }
            finally
            {
                writer.Complete();
            }

            return Ok(); // This line will likely never be reached
        }

        [HttpPost("login")]
        [Produces("application/json")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var loginResult = await _authservice.Login(model);
            return Ok(loginResult);
        }
    }
}
