using CraneFileManager.Application.Mapper.DTO.NotificationDTO;
using CraneFileManager.Application.Mapper.DTO.UserNotificationDTO;
using System.Security.Claims;

namespace CraneFileManager.Application.Services.Abstract
{
    public interface INotificationService
    {
        #region Notification service

        public Task CreateNotification(NotificationDTOforCreate model, ClaimsPrincipal claimsPrincipal);

        public Task UpdateNotification(NotificationDTOforUpdate model, ClaimsPrincipal claimsPrincipal);

        public Task DeleteNotification(Guid Id, ClaimsPrincipal claimsPrincipal);

        public Task<List<NotificationDTOforGetandGetAll>> GetNotifications(ClaimsPrincipal claimsPrincipal);

        public Task<NotificationDTOforGetandGetAll> GetNotificationById(Guid Id, ClaimsPrincipal claimsPrincipal);




        #endregion
    }

}
