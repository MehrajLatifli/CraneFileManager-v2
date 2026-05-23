using AutoMapper;
using CraneFileManager.Application.Mapper.DTO.AuthDTO;
using CraneFileManager.Application.Mapper.DTO.FileDTO;
using CraneFileManager.Application.Mapper.DTO.FileShareDTO;
using CraneFileManager.Application.Mapper.DTO.FileTrashCanDTO;
using CraneFileManager.Application.Mapper.DTO.FileTypeDTO;
using CraneFileManager.Application.Mapper.DTO.NotificationDTO;
using CraneFileManager.Application.Mapper.DTO.RoleClaimDTO;
using CraneFileManager.Application.Mapper.DTO.RoleDTO;
using CraneFileManager.Application.Mapper.DTO.RolePermissionDTO;
using CraneFileManager.Application.Mapper.DTO.UserClaimDTO;
using CraneFileManager.Application.Mapper.DTO.UserDTO;
using CraneFileManager.Application.Mapper.DTO.UserFileDTO;
using CraneFileManager.Application.Mapper.DTO.UserNotificationDTO;
using CraneFileManager.Application.Mapper.DTO.UserPermissionDTO;
using CraneFileManager.Application.Mapper.DTO.UserRoleDTO;
using CraneFileManager.Domain;
using CraneFileManager.Domain.Entities.AuthModels;
using CraneFileManager.Domain.Entities.IdentityAuth;
using CraneFileManager.Domain.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Register, RegisterDTO>();
            CreateMap<RegisterDTO, Register>();

            CreateMap<Login, LoginDTO>();
            CreateMap<LoginDTO, Login>();

            CreateMap<LoginResponse, LoginResponseDTO>();
            CreateMap<LoginResponseDTO, LoginResponse>();

            CreateMap<Login2FA, LoginDTO2FA>();
            CreateMap<LoginDTO2FA, Login2FA>();

            CreateMap<LoginResponse2FA, LoginResponseDTO2FA>();
            CreateMap<LoginResponseDTO2FA, LoginResponse2FA>();

            CreateMap<UpdatePassword, UpdatePasswordDTO>();
            CreateMap<UpdatePasswordDTO, UpdatePassword>();

            CreateMap<UpdateProfile, UpdateProfileDTO>();
            CreateMap<UpdateProfileDTO, UpdateProfile>();

            CreateMap<UpdateUserBlockStatus, UpdateUserBlockStatusDTO>();
            CreateMap<UpdateUserBlockStatusDTO, UpdateUserBlockStatus>();

            CreateMap<User, UserDTOforCreate>();
            CreateMap<UserDTOforCreate, User>();
            CreateMap<User, UserDTOforUpdate>();
            CreateMap<UserDTOforUpdate, User>();
            CreateMap<User, UserDTOforGetandGetAll>();
            CreateMap<UserDTOforGetandGetAll, User>();

            CreateMap<Role, RoleDTOforCreate>();
            CreateMap<RoleDTOforCreate, Role>();
            CreateMap<Role, RoleDTOforUpdate>();
            CreateMap<RoleDTOforUpdate, Role>();
            CreateMap<Role, RoleDTOforGetandGetAll>();
            CreateMap<RoleDTOforGetandGetAll, Role>();

            CreateMap<UserRole, UserRoleDTOforCreate>();
            CreateMap<UserRoleDTOforCreate, UserRole>();
            CreateMap<UserRole, UserRoleDTOforUpdate>();
            CreateMap<UserRoleDTOforUpdate, UserRole>();
            CreateMap<UserRole, UserRoleDTOforGetandGetAll>();
            CreateMap<UserRoleDTOforGetandGetAll, UserRole>();

            CreateMap<RolePermission, RolePermissionDTOforCreate>();
            CreateMap<RolePermissionDTOforCreate, RolePermission>();
            CreateMap<RolePermission, RolePermissionDTOforUpdate>();
            CreateMap<RolePermissionDTOforUpdate, RolePermission>();
            CreateMap<RolePermission, RolePermissionDTOforGetandGetAll>();
            CreateMap<RolePermissionDTOforGetandGetAll, RolePermission>();

            CreateMap<UserPermission, UserPermissionDTOforCreate>();
            CreateMap<UserPermissionDTOforCreate, UserPermission>();
            CreateMap<UserPermission, UserPermissionDTOforUpdate>();
            CreateMap<UserPermissionDTOforUpdate, UserPermission>();
            CreateMap<UserPermission, UserPermissionDTOforGetandGetAll>();
            CreateMap<UserPermissionDTOforGetandGetAll, UserPermission>();

            CreateMap<RoleClaim, RoleClaimDTOforCreate>();
            CreateMap<RoleClaimDTOforCreate, RoleClaim>();
            CreateMap<RoleClaim, RoleClaimDTOforUpdate>();
            CreateMap<RoleClaimDTOforUpdate, RoleClaim>();
            CreateMap<RoleClaim, RoleClaimDTOforGetandGetAll>();
            CreateMap<RoleClaimDTOforGetandGetAll, RoleClaim>();

            CreateMap<UserClaim, UserClaimDTOforCreate>();
            CreateMap<UserClaimDTOforCreate, UserClaim>();
            CreateMap<UserClaim, UserClaimDTOforUpdate>();
            CreateMap<UserClaimDTOforUpdate, UserClaim>();
            CreateMap<UserClaim, UserClaimDTOforGetandGetAll>();
            CreateMap<UserClaimDTOforGetandGetAll, UserClaim>();

            CreateMap<Notification, NotificationDTOforCreate>();
            CreateMap<NotificationDTOforCreate, Notification>();
            CreateMap<Notification, NotificationDTOforUpdate>();
            CreateMap<NotificationDTOforUpdate, Notification>();
            CreateMap<Notification, NotificationDTOforGetandGetAll>();
            CreateMap<NotificationDTOforGetandGetAll, Notification>();

            CreateMap<UserNotification, UserNotificationDTOforCreate>();
            CreateMap<UserNotificationDTOforCreate, UserNotification>();
            CreateMap<UserNotification, UserNotificationDTOforUpdate>();
            CreateMap<UserNotificationDTOforUpdate, UserNotification>();
            CreateMap<UserNotification, UserNotificationDTOforGetandGetAll>();
            CreateMap<UserNotificationDTOforGetandGetAll, UserNotification>();

            CreateMap<Domain.Entities.Models.File, FileDTOforCreate>();
            CreateMap<FileDTOforCreate, Domain.Entities.Models.File>();
            CreateMap<Domain.Entities.Models.File, FileDTOforUpdate>();
            CreateMap<FileDTOforUpdate, Domain.Entities.Models.File>();
            CreateMap<Domain.Entities.Models.File, FileDTOforGetandGetAll>()/*.IgnoreNoMap()*/;
            //CreateMap<FileDTOforGetandGetAll, Domain.Entities.Models.File>().ForMember(dest => dest.FileTypeId, opt => opt.Ignore());
            //CreateMap<FileDTOforGetandGetAll, Domain.Entities.Models.File>().IgnoreNoMap();
            //CreateMap<Domain.Entities.Models.File, FileDTOforGetandGetAll>();
            CreateMap<FileDTOforGetandGetAll, Domain.Entities.Models.File>();

            CreateMap<Domain.Entities.Models.FileShare, FileShareDTOforCreate>();
            CreateMap<FileShareDTOforCreate, Domain.Entities.Models.FileShare>();
            CreateMap<Domain.Entities.Models.FileShare, FileShareDTOforUpdate>();
            CreateMap<FileShareDTOforUpdate, Domain.Entities.Models.FileShare>();
            CreateMap<Domain.Entities.Models.FileShare, FileShareDTOforGetandGetAll>();
            CreateMap<FileShareDTOforGetandGetAll, Domain.Entities.Models.FileShare>();

            CreateMap<FileType, FileTypeDTOforCreate>();
            CreateMap<FileTypeDTOforCreate, FileType>();
            CreateMap<FileType, FileTypeDTOforUpdate>();
            CreateMap<FileTypeDTOforUpdate, FileType>();
            CreateMap<FileType, FileTypeDTOforGetandGetAll>();
            CreateMap<FileTypeDTOforGetandGetAll, FileType>();

            CreateMap<UserFile, UserFileDTOforCreate>();
            CreateMap<UserFileDTOforCreate, UserFile>();
            CreateMap<UserFile, UserFileDTOforUpdate>();
            CreateMap<UserFileDTOforUpdate, UserFile>();
            CreateMap<UserFile, UserFileDTOforGetandGetAll>();
            CreateMap<UserFileDTOforGetandGetAll, UserFile>();

            CreateMap<FileTrashCan, FileTrashCanDTOforCreate>();
            CreateMap<FileTrashCanDTOforCreate, FileTrashCan>();
            CreateMap<FileTrashCan, FileTrashCanDTOforUpdate>();
            CreateMap<FileTrashCanDTOforUpdate, FileTrashCan>();
            CreateMap<FileTrashCan, FileTrashCanDTOforGetandGetAll>();
            CreateMap<FileTrashCanDTOforGetandGetAll, FileTrashCan>();


        }
    }
}
