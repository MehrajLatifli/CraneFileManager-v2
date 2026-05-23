using Asp.Versioning;
using AutoMapper;
using CraneFileManager.Application.Cache.RedisCachePatterns.Abstract;
using CraneFileManager.Application.Exceptions;
using CraneFileManager.Application.Mapper.DTO.AuthDTO;
using CraneFileManager.Application.Mapper.DTO.FileDTO;
using CraneFileManager.Application.Mapper.DTO.FileTrashCanDTO;
using CraneFileManager.Application.MessageBroker.RabbitMQ.Abstract;
using CraneFileManager.Application.MessageBroker.RabbitMQ.Custom;
using CraneFileManager.Application.Services.Abstract;
using CraneFileManager.Application.Validations;
using CraneFileManager.Domain.Entities.AuthModels;
using CraneFileManager.Infrastructure.SignalR;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using CraneFileManager.Persistence.ServiceExtensions;
using CraneFileManager.User.API.API_Routes;
using CraneFileManager.User.API.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;

namespace CraneFileManager.User.API.Controllers.Files.File
{
    [ApiVersion(1, Deprecated = false)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IAuthService _authservice;
        private readonly IFileService _fileservice;
        private readonly INotificationService _notificationService;
        private readonly CraneFileManagerContext _craneFileManagerContext;
        private readonly NotificationHubService _notificationHubService;
        private readonly IFileCacheService<FileDTOforGetandGetAll> _cacheServiceFileDTOforGetandGetAll;
        private readonly IFileCacheService<FileDTOforUpdate> _cacheServiceFileDTOforUpdate;
        private readonly IFileCacheService<FileDTOforCreate> _cacheServiceFileDTOforCreate;

        private readonly IFileTrashCanCacheService<FileTrashCanDTOforGetandGetAll> _cacheServiceFileTrashCanDTOforGetandGetAll;
        private readonly IFileTrashCanCacheService<FileTrashCanDTOforUpdate> _cacheServiceFileTrashCanDTOforUpdate;
        private readonly IFileTrashCanCacheService<FileTrashCanDTOforCreate> _cacheServiceFileTrashCanDTOforCreate;

        private readonly IConfiguration _configuration;

        private readonly EventBuses _eventBuses;

        public FileController(IMapper mapper, IAuthService authservice, IFileService fileservice, INotificationService notificationService, CraneFileManagerContext craneFileManagerContext, NotificationHubService notificationHubService, EventBuses eventBuses, IFileCacheService<FileDTOforGetandGetAll> cacheServiceFileDTOforGetandGetAll, IFileCacheService<FileDTOforUpdate> cacheServiceFileDTOforUpdate, IFileCacheService<FileDTOforCreate> cacheServiceFileDTOforCreate, IConfiguration configuration, IFileTrashCanCacheService<FileTrashCanDTOforGetandGetAll> cacheServiceFileTrashCanDTOforGetandGetAll, IFileTrashCanCacheService<FileTrashCanDTOforUpdate> cacheServiceFileTrashCanDTOforUpdate, IFileTrashCanCacheService<FileTrashCanDTOforCreate> cacheServiceFileTrashCanDTOforCreate)
        {
            _mapper = mapper;
            _authservice = authservice;
            _fileservice = fileservice;
            _notificationService = notificationService;
            _craneFileManagerContext = craneFileManagerContext;
            _notificationHubService = notificationHubService;
            _eventBuses = eventBuses;
            _cacheServiceFileDTOforGetandGetAll = cacheServiceFileDTOforGetandGetAll;
            _cacheServiceFileDTOforUpdate = cacheServiceFileDTOforUpdate;
            _cacheServiceFileDTOforCreate = cacheServiceFileDTOforCreate;
            _configuration = configuration;
            _cacheServiceFileTrashCanDTOforGetandGetAll = cacheServiceFileTrashCanDTOforGetandGetAll;
            _cacheServiceFileTrashCanDTOforUpdate = cacheServiceFileTrashCanDTOforUpdate;
            _cacheServiceFileTrashCanDTOforCreate = cacheServiceFileTrashCanDTOforCreate;
        }

        [HttpPost]
        [Route(Routes.File)]
        [Produces("application/json")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin, UserRoles.User }, CustomRolePermissions = new[] { "Post_FileForAdmin", "Post_FileForUser" }, CustomUserPermissions = new[] { "Create" })]

        public async Task<IActionResult> FileCreate([FromForm] UploadFileDTO model)
        {

          
                await _eventBuses.FileCreateEvent(model, "FileCreate_exchange", "FileCreate_notification", User);


            return Ok(new Response { Status = "Success", Message = $"{model.Name.FileName} uploaded successfully!" });
           
        }

        [HttpGet]
        [Route(Routes.File)]
        [Produces("application/json")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin, UserRoles.User }, CustomRolePermissions = new[] { "Get_FilesForAdmin", "Get_FilesForUser" }, CustomUserPermissions = new[] { "Read" })]

        public async Task<IActionResult> GetFiles()
        {


            await _eventBuses.FilesReadEvent("FileGet_exchange", "FileGet_notification", User);

            await Task.Delay(2000);

            var cachedFiles = await _cacheServiceFileDTOforGetandGetAll.GetAllFilesByUser(User.Identity.Name);

            if (cachedFiles.Count > 0)
            {
                var items = _mapper.Map<List<FileDTOforGetandGetAll>>(cachedFiles);

                return Ok(items.OrderBy(o => o.CreatedDate).ToList());
            }

            else
            {
                var files = await _fileservice.ViewFiles(User.Identity.Name);

                var items = _mapper.Map<List<FileDTOforGetandGetAll>>(files);

                return Ok(_mapper.Map<List<FileDTOforGetandGetAll>>(items.OrderBy(o => o.CreatedDate).ToList()));
            }






        }

        [HttpPut]
        [Route(Routes.File)]
        [Produces("application/json")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin, UserRoles.User }, CustomRolePermissions = new[] { "Put_FileForAdmin", "Put_FileForUser" }, CustomUserPermissions = new[] { "Update" })]

        public async Task<IActionResult> FileUpdate([FromForm] FileDTOforUpdate model)
        {


            await _eventBuses.FilesUpdateEvent(model, "FileUpdate_exchange", "FileUpdate_notification", User);

            //await _fileservice.UpdateFile(model.Id, model.DisplayName, _configuration["ConnectionAzureStorage"], User.Identity.Name);

            return Ok(new Response { Status = "Success", Message = $"{model.DisplayName} updated successfully!" });

        }


        [HttpPost]
        [Route(Routes.AddFileTrashCan)]
        [Produces("application/json")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin, UserRoles.User }, CustomRolePermissions = new[] { "Post_FileTrashCanForAdmin", "Post_FileTrashCanForUser" }, CustomUserPermissions = new[] { "Create" })]

        public async Task<IActionResult> AddFileTrashCan(Guid Id)
        {



            await _eventBuses.AddFileTrashCanEvent(Id, "FileTrashCanCreate_exchange", "FileTrashCanCreate_notification", User);


            // await _fileservice.AddTrashCan(Id, _configuration["ConnectionAzureStorage"], User.Identity.Name);


            return Ok(new Response { Status = "Success", Message = $"File with id {Id} has been added to the TrashCan" });

        }


        [HttpPut]
        [Route(Routes.AddFileTrashCan)]
        [Produces("application/json")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin, UserRoles.User }, CustomRolePermissions = new[] { "Put_FileTrashCanForAdmin", "Put_FileTrashCanForUser" }, CustomUserPermissions = new[] { "Update" })]

        public async Task<IActionResult> UpdateFileTrashCan(Guid Id)
        {

            await _eventBuses.UpdateFileTrashCanEvent(Id, "FileTrashCanUpdate_exchange", "FileTrashCanUpdate_notification", User);

           // await _fileservice.UpdateTrashCan(Id, _configuration["ConnectionAzureStorage"], User.Identity.Name);

            return Ok(new Response { Status = "Success", Message = $"File with id {Id} has been updated in the TrashCan" });

        }


        [HttpGet]
        [Route(Routes.FileTrashCan)]
        [Produces("application/json")]
        [CustomAuthorize(CustomRoles = new[] { UserRoles.Admin, UserRoles.User }, CustomRolePermissions = new[] { "Get_FileTrashCanForAdmin", "Get_FileTrashCanForUser" }, CustomUserPermissions = new[] { "Read" })]

        public async Task<IActionResult> GetFilesInFileTrashCan()
        {


            await _eventBuses.FilesReadInTrashCanEvent("FileInTrashCanGet_exchange", "FileInTrashCanGet_notification", User);

           



            await Task.Delay(2000);

            var cachedFiles = await _cacheServiceFileDTOforGetandGetAll.GetAllFilesInTrashCanByUser(User.Identity.Name);

            if (cachedFiles.Count > 0)
            {
        
           

                var items = _mapper.Map<List<FileDTOforGetandGetAll>>(cachedFiles);

                return Ok(items.OrderBy(o => o.CreatedDate).ToList());
            }

            else
            {
                var files = await _fileservice.ViewFilesInTrashCan(User.Identity.Name);

                var items = _mapper.Map<List<FileDTOforGetandGetAll>>(files);

                return Ok(_mapper.Map<List<FileDTOforGetandGetAll>>(items.OrderBy(o => o.CreatedDate).ToList()));
            }






        }
    }
}
