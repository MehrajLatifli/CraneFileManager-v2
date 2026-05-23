using AutoMapper;
using CraneFileManager.Application.Cache.RedisCachePatterns.Abstract;
using CraneFileManager.Application.Mapper.DTO.AuthDTO;
using CraneFileManager.Application.Mapper.DTO.NotificationDTO;
using CraneFileManager.Application.Mapper.DTO.UserDTO;
using CraneFileManager.Application.Mapper.DTO.UserNotificationDTO;
using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Application.Services.Abstract;
using CraneFileManager.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Services.Concrete
{
    public class NotificationServiceManager : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationServiceManager> _logger;

        private readonly IUserWriteRepository _userWriteRepository;
        private readonly IUserReadRepository _userReadRepository;
        private readonly INotificationWriteRepository _notificationWriteRepository;
        private readonly IUserNotificationWriteRepository _userNotificationWriteRepository;
        private readonly INotificationReadRepository  _notificationReadRepository;
        private readonly IUserNotificationReadRepository  _userNotificationReadRepository;

        public NotificationServiceManager(IConfiguration configuration, IMapper mapper, ILogger<NotificationServiceManager> logger, INotificationWriteRepository notificationWriteRepository, IUserNotificationWriteRepository userNotificationWriteRepository, INotificationReadRepository notificationReadRepository, IUserNotificationReadRepository userNotificationReadRepository, IUserWriteRepository userWriteRepository, IUserReadRepository userReadRepository)
        {
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
            _notificationWriteRepository = notificationWriteRepository;
            _userNotificationWriteRepository = userNotificationWriteRepository;
            _notificationReadRepository = notificationReadRepository;
            _userNotificationReadRepository = userNotificationReadRepository;
            _userWriteRepository = userWriteRepository;
            _userReadRepository = userReadRepository;
        }

        public Task CreateNotification(NotificationDTOforCreate model, ClaimsPrincipal claimsPrincipal)
        {
            throw new NotImplementedException();
        }

        public Task DeleteNotification(Guid Id, ClaimsPrincipal claimsPrincipal)
        {
            throw new NotImplementedException();
        }

        public Task<NotificationDTOforGetandGetAll> GetNotificationById(Guid Id, ClaimsPrincipal claimsPrincipal)
        {
            throw new NotImplementedException();
        }



        public async Task<List<NotificationDTOforGetandGetAll>> GetNotifications(ClaimsPrincipal claimsPrincipal)
        {

            var users = _mapper.Map<List<UserDTOforGetandGetAll>>(await _userReadRepository.GetAll(false).ToListAsync());


            var notifications = await _notificationReadRepository.GetAll(false).ToListAsync();

            if (notifications.Any())
            {
               
                var notificationDtos = _mapper.Map<List<NotificationDTOforGetandGetAll>>(notifications);
                return notificationDtos; 
            }
            else
            {

                return new List<NotificationDTOforGetandGetAll>(); 
            }
        }
        public Task UpdateNotification(NotificationDTOforUpdate model, ClaimsPrincipal claimsPrincipal)
        {
            throw new NotImplementedException();
        }
    }
}
