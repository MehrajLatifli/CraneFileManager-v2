using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using CraneFileManager.Application.Cache.RedisCachePatterns.Abstract;
using CraneFileManager.Application.Exceptions;
using CraneFileManager.Application.Mapper.DTO.FileDTO;
using CraneFileManager.Application.Mapper.DTO.FileTrashCanDTO;
using CraneFileManager.Application.Mapper.DTO.FileTypeDTO;
using CraneFileManager.Application.Mapper.DTO.UserDTO;
using CraneFileManager.Application.Mapper.DTO.UserFileDTO;
using CraneFileManager.Application.MessageBroker.RabbitMQ.Custom;
using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Domain.Entities.Configurations;
using CraneFileManager.Domain.Entities.Models;
using CraneFileManager.Infrastructure.RabbitMQPattern;
using CraneFileManager.Persistence.Repositories.Custom;
using CraneFileManager.Persistence.ServiceExtensions;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using MongoDB.Driver.Core.Configuration;
using Newtonsoft.Json;

namespace CraneFileManager.User.API.Events
{
    public class EventBuses
    {
        private readonly IMapper _mapper;
        private readonly IUserReadRepository _userReadRepository;
        private readonly IRabbitMQService _rabbitMQService;
        private readonly IFileCacheService<FileDTOforGetandGetAll> _cacheServiceFileDTOforGetandGetAll;
        private readonly IFileReadRepository _fileReadRepository;
        private readonly IFileTrashCanReadRepository _fileTrashCanReadRepository;
        private readonly IFileTypeReadRepository _fileTypeReadRepository;

        private readonly IUserFileReadRepository _userFileReadRepository;
        private readonly AppSettings _appSettings;

        public EventBuses(
    IMapper mapper,
    IUserReadRepository userReadRepository,
    IRabbitMQService rabbitMQService,
    IFileCacheService<FileDTOforGetandGetAll> cacheServiceFileDTOforGetandGetAll,
    IFileReadRepository fileReadRepository,
    AppSettings appSettings,
    IUserFileReadRepository userFileReadRepository,
    IFileTrashCanReadRepository fileTrashCanReadRepository,
    IFileTypeReadRepository fileTypeReadRepository)
        {
            _mapper = mapper;
            _userReadRepository = userReadRepository;
            _rabbitMQService = rabbitMQService;
            _cacheServiceFileDTOforGetandGetAll = cacheServiceFileDTOforGetandGetAll;
            _fileReadRepository = fileReadRepository;
            _appSettings = appSettings;
            _userFileReadRepository = userFileReadRepository;
            _fileTrashCanReadRepository = fileTrashCanReadRepository;
            _fileTypeReadRepository = fileTypeReadRepository;
        }

        private async Task ValidateUserAuthorization(ClaimsPrincipal claimsPrincipal)
        {
            if (!claimsPrincipal.Identity.IsAuthenticated)
            {
                throw new UnauthorizedException("Current user is not authenticated.");
            }

            var users = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false));
            if (users.Any(i => string.IsNullOrEmpty(i.RefreshToken) && i.Username == claimsPrincipal.Identity.Name))
            {
                throw new UnauthorizedException("Current user is not authenticated.");
            }
        }

        private async Task ValidateFiles(bool IsRemove, string CurrentUser)
        {
            var files = _mapper.Map<List<FileDTOforGetandGetAll>>(_fileReadRepository.GetAll(false));
            var userFiles = _mapper.Map<List<UserFileDTOforGetandGetAll>>(_userFileReadRepository.GetAll(false));
            var users = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false));
            var user = users.FirstOrDefault(u => u.Username == CurrentUser);

            var filteredFiles = (from u in users
                                 join uf in userFiles on u.Id equals uf.UserId
                                 join f in files on uf.FileId equals f.Id
                                 where u.Username == CurrentUser && f.IsRemove == IsRemove
                                 select f).OrderBy(o => o.CreatedDate).ToList();

            if (filteredFiles.Count <= 0)
            {
                throw new NotFoundException("File not found.");
            }
        }

        private async Task ValidateFilesInTrashcan(bool IsRemove, string CurrentUser)
        {
            var users = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false));
            var files = _mapper.Map<List<FileDTOforGetandGetAll>>(_fileReadRepository.GetAll(false));
            var userFiles = _mapper.Map<List<UserFileDTOforGetandGetAll>>(_userFileReadRepository.GetAll(false));
            var fileTrashCans = _mapper.Map<List<FileTrashCanDTOforGetandGetAll>>(_fileTrashCanReadRepository.GetAll(false));
            var fileTypes = _mapper.Map<List<FileTypeDTOforGetandGetAll>>(_fileTypeReadRepository.GetAll(false));

            var filteredFiles = (from u in users
                                 join uf in userFiles on u.Id equals uf.UserId
                                 join f in files on uf.FileId equals f.Id
                                 where u.Username == CurrentUser && f.IsRemove == IsRemove
                                 select f).OrderBy(o => o.CreatedDate).ToList();

         
            if (filteredFiles.Count <=0)
            {
                throw new NotFoundException("File not found.");
            }

            var filteredFileTrashCans = (from ftc in fileTrashCans
                                         join f in files on ftc.FileId equals f.Id
                                         join uf in userFiles on f.Id equals uf.FileId
                                         join u in users on uf.UserId equals u.Id
                                         join ft in fileTypes on f.FileTypeId equals ft.Id
                                         where u.Username == CurrentUser && ftc.ThrowTrashDate != null
                                         select new
                                         {
                                             f.Id,
                                             f.OrginalName,
                                             f.DisplayName,
                                             f.Description,
                                             f.IsRemove,
                                             f.Size,
                                             f.Path,
                                             CreatedDate = f.CreatedDate,  // Format as string
                                             UpdatedDate = f.UpdatedDate,  // Format as string
                                             FileType = ft.Type
                                         }).OrderBy(o => o.CreatedDate).ToList();

            if (filteredFileTrashCans.Count <= 0)
            {
                throw new NotFoundException("File not found in TrashCan.");
            }


        }

        private async Task ValidateTrashFiles(bool IsRemove, string CurrentUser, Guid Id)
        {
            var files = _mapper.Map<List<FileDTOforGetandGetAll>>(_fileReadRepository.GetAll(false));
            var userFiles = _mapper.Map<List<UserFileDTOforGetandGetAll>>(_userFileReadRepository.GetAll(false));
            var users = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false));
            var user = users.FirstOrDefault(u => u.Username == CurrentUser);

            var filteredFiles = (from u in users
                                 join uf in userFiles on u.Id equals uf.UserId
                                 join f in files on uf.FileId equals f.Id
                                 where u.Username == CurrentUser && f.IsRemove == IsRemove
                                 select f).OrderBy(o => o.CreatedDate).ToList();

            var fileToUpdate = filteredFiles.FirstOrDefault(i => i.Id == Id);
            if (fileToUpdate == null)
            {
                throw new NotFoundException("File not found.");
            }
        }

        public async Task FilesReadEvent(string eventname, string eventtype, ClaimsPrincipal claimsPrincipal)
        {
            var validateUserTask = ValidateUserAuthorization(claimsPrincipal);


            var validateFiles = ValidateFiles(false, claimsPrincipal.Identity.Name);


            await Task.WhenAll(validateUserTask, validateFiles);




            var fileEvent = new FileEventType
            {
                Id = Guid.NewGuid(),
                OrginalName = string.Empty,
                DisplayName = string.Empty,
                Eventname = eventname,
                Eventtype = eventtype,
                CurrentUser = claimsPrincipal.Identity.Name,
                IsIdentity = claimsPrincipal.Identity.IsAuthenticated,
                FileContent = new byte[0]
            };

            await _rabbitMQService.PublishMessage(JsonConvert.SerializeObject(fileEvent), fileEvent.Eventname, fileEvent.Eventtype);
        }

        public async Task FilesReadInTrashCanEvent(string eventname, string eventtype, ClaimsPrincipal claimsPrincipal)
        {
            var validateUserTask = ValidateUserAuthorization(claimsPrincipal);


            var validateFiles = ValidateFiles(false, claimsPrincipal.Identity.Name);

            var validateFilesInTrashcan = ValidateFilesInTrashcan(true, claimsPrincipal.Identity.Name);

            await Task.WhenAll(validateUserTask, validateFiles, validateFilesInTrashcan);




            var fileEvent = new FileEventType
            {
                Id = Guid.NewGuid(),
                OrginalName = string.Empty,
                DisplayName = string.Empty,
                Eventname = eventname,
                Eventtype = eventtype,
                CurrentUser = claimsPrincipal.Identity.Name,
                IsIdentity = claimsPrincipal.Identity.IsAuthenticated,
                FileContent = new byte[0]
            };

            await _rabbitMQService.PublishMessage(JsonConvert.SerializeObject(fileEvent), fileEvent.Eventname, fileEvent.Eventtype);
        }

        public async Task FilesUpdateEvent(FileDTOforUpdate model, string eventname, string eventtype, ClaimsPrincipal claimsPrincipal)
        {
           var validateUserTask =  ValidateUserAuthorization(claimsPrincipal);


            var validateFiles = ValidateFiles(false, claimsPrincipal.Identity.Name);

            await Task.WhenAll(validateUserTask, validateFiles);

            var user = _userReadRepository.GetAll(false).FirstOrDefault(u => u.Username == claimsPrincipal.Identity.Name);

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            var files = _mapper.Map<List<FileDTOforGetandGetAll>>(_fileReadRepository.GetAll(false));

            if (!files.Any(i => i.Id == model.Id))
            {
                throw new NotFoundException("File not found.");
            }


            System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();
            TimeZone localZone = TimeZone.CurrentTimeZone;
            DateTime localTime = localZone.ToLocalTime(DateTime.UtcNow);


            var fileEvent = new FileEventType
            {
                Id = model.Id,
                OrginalName = string.Empty,
                DisplayName = model.DisplayName,
                Eventname = eventname,
                Eventtype = eventtype,
                CurrentUser = claimsPrincipal.Identity.Name,
                IsIdentity = claimsPrincipal.Identity.IsAuthenticated,
                FileContent = new byte[0],
                UpdatedDate = localTime
            };

            await _rabbitMQService.PublishMessage(JsonConvert.SerializeObject(fileEvent), fileEvent.Eventname, fileEvent.Eventtype);
        }

        public async Task AddFileTrashCanEvent(Guid Id, string eventname, string eventtype, ClaimsPrincipal claimsPrincipal)
        {
            var validateUserTask = ValidateUserAuthorization(claimsPrincipal);
            var validateTrashFilesTask = ValidateTrashFiles(false, claimsPrincipal.Identity.Name, Id);

            // Wait for all tasks to complete
            await Task.WhenAll(validateUserTask, validateTrashFilesTask);








            System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();
            TimeZone localZone = TimeZone.CurrentTimeZone;
            DateTime localTime = localZone.ToLocalTime(DateTime.UtcNow);


            var fileTrashCanEvent = new FileTrashCanEventType
            {
                Id = Id,
                ThrowTrashDate = localTime,

                Eventname = eventname,
                Eventtype = eventtype,
                CurrentUser = claimsPrincipal.Identity.Name,
                IsIdentity = claimsPrincipal.Identity.IsAuthenticated,

            };

            await _rabbitMQService.PublishMessage(JsonConvert.SerializeObject(fileTrashCanEvent), fileTrashCanEvent.Eventname, fileTrashCanEvent.Eventtype);
        }

        public async Task UpdateFileTrashCanEvent(Guid Id, string eventname, string eventtype, ClaimsPrincipal claimsPrincipal)
        {
            var validateUserTask = ValidateUserAuthorization(claimsPrincipal);
            var validateTrashFilesTask = ValidateTrashFiles(true, claimsPrincipal.Identity.Name, Id);

            // Wait for all tasks to complete
            await Task.WhenAll(validateUserTask, validateTrashFilesTask);

            var files = _mapper.Map<List<FileDTOforGetandGetAll>>(_fileReadRepository.GetAll(false));
            var userFiles = _mapper.Map<List<UserFileDTOforGetandGetAll>>(_userFileReadRepository.GetAll(false));
            var users = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false));
            var user = users.FirstOrDefault(u => u.Username == claimsPrincipal.Identity.Name);

            var filteredFiles = (from u in users
                                 join uf in userFiles on u.Id equals uf.UserId
                                 join f in files on uf.FileId equals f.Id
                                 where u.Username == claimsPrincipal.Identity.Name && f.IsRemove == true
                                 select f).OrderBy(o => o.CreatedDate).ToList();

            var fileToUpdate = filteredFiles.FirstOrDefault(i => i.Id == Id);
            if (fileToUpdate == null)
            {
                throw new NotFoundException("File not found.");
            }

            System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();
            TimeZone localZone = TimeZone.CurrentTimeZone;
            DateTime localTime = localZone.ToLocalTime(DateTime.UtcNow);


            var fileTrashCanEvent = new FileTrashCanEventType
            {
                Id = Id,
                TakeofTrashDate = localTime,

                Eventname = eventname,
                Eventtype = eventtype,
                CurrentUser = claimsPrincipal.Identity.Name,
                IsIdentity = claimsPrincipal.Identity.IsAuthenticated,

            };

            await _rabbitMQService.PublishMessage(JsonConvert.SerializeObject(fileTrashCanEvent), fileTrashCanEvent.Eventname, fileTrashCanEvent.Eventtype);
        }

        private static Dictionary<Guid, List<FileEventType>> _chunkStore = new Dictionary<Guid, List<FileEventType>>();

        public async Task FileCreateEvent(UploadFileDTO model, string eventname, string eventtype, ClaimsPrincipal claimsPrincipal)
        {
            await ValidateUserAuthorization(claimsPrincipal);

            const int chunkSize = 50 * 1024 * 1024; byte[] buffer = new byte[chunkSize];
            int bytesRead;
            int chunkIndex = 0;
            var fileId = Guid.NewGuid();
            var fileEvent = new FileEventType();

            using (var stream = model.Name.OpenReadStream())
            {
                var totalChunks = (int)Math.Ceiling(stream.Length / (double)chunkSize);

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    fileEvent = new FileEventType
                    {
                        Id = fileId,
                        OrginalName = $"{claimsPrincipal.Identity.Name}_{fileId}{Path.GetExtension(model.Name.FileName)}",
                        DisplayName = model.Name.FileName,
                        Eventname = eventname,
                        Eventtype = eventtype,
                        CurrentUser = claimsPrincipal.Identity.Name,
                        IsIdentity = claimsPrincipal.Identity.IsAuthenticated,
                        ChunkIndex = chunkIndex,
                        TotalChunks = totalChunks,
                        FileContent = buffer.Take(bytesRead).ToArray(),
                        Path = string.Empty,
                        Size = 0
                    };

                    if (!_chunkStore.ContainsKey(fileId))
                    {
                        _chunkStore[fileId] = new List<FileEventType>();
                    }
                    _chunkStore[fileId].Add(fileEvent);

                    chunkIndex++;
                }

                var combinedFile = await CombineFileChunksAsync(fileId);

                var (uploadUrl, fileSize) = await UploadToAzureBlobStorageAsync(combinedFile, fileId, claimsPrincipal.Identity.Name, fileEvent.OrginalName);

                fileEvent.Path = uploadUrl;
                fileEvent.Size = fileSize;

                await _rabbitMQService.PublishMessage(JsonConvert.SerializeObject(fileEvent), fileEvent.Eventname, fileEvent.Eventtype);
            }
        }

        public async Task<byte[]> CombineFileChunksAsync(Guid fileId)
        {
            if (!_chunkStore.ContainsKey(fileId))
            {
                throw new Exception("File chunks not found.");
            }

            using (var memoryStream = new MemoryStream())
            {
                var orderedChunks = _chunkStore[fileId].OrderBy(c => c.ChunkIndex).ToList();
                foreach (var chunk in orderedChunks)
                {
                    await memoryStream.WriteAsync(chunk.FileContent, 0, chunk.FileContent.Length);
                }
                return memoryStream.ToArray();
            }
        }

        public async Task<(string fileUrl, long fileSize)> UploadToAzureBlobStorageAsync(byte[] fileContent, Guid fileId, string currentUser, string filename)
        {
            string connectionString = _appSettings.ConnectionAzureStorage;
            string containerName = "user-files";
            string userFolder = $"{currentUser}/"; string blobName = $"{userFolder}{currentUser}_{fileId}{Path.GetExtension(filename)}";
            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = GetContentType(Path.GetExtension(filename)),
                ContentDisposition = "inline"
            };

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            var blockBlobClient = containerClient.GetBlockBlobClient(blobName);

            var blockIds = new List<string>(); var uploadTasks = new List<Task>();
            const int chunkSize = 50 * 1024 * 1024; int numChunks = (int)Math.Ceiling((double)fileContent.Length / chunkSize);

            var maxConcurrentUploads = 8;

            for (int i = 0; i < numChunks; i++)
            {
                var chunkData = fileContent.Skip(i * chunkSize).Take(chunkSize).ToArray();
                var blockId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                blockIds.Add(blockId);

                var uploadTask = Task.Run(async () =>
                {
                    using (var memoryStream = new MemoryStream(chunkData))
                    {
                        await blockBlobClient.StageBlockAsync(blockId, memoryStream);
                    }
                });

                uploadTasks.Add(uploadTask);

                if (uploadTasks.Count >= maxConcurrentUploads)
                {
                    await Task.WhenAny(uploadTasks);
                    uploadTasks.RemoveAll(t => t.IsCompleted);
                }
            }

            await Task.WhenAll(uploadTasks);

            await blockBlobClient.CommitBlockListAsync(blockIds);

            var blobProperties = await blockBlobClient.GetPropertiesAsync();
            long fileSize = blobProperties.Value.ContentLength;

            return (blockBlobClient.Uri.AbsoluteUri, fileSize);

        }

        private string GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".tar" => "application/x-tar",
                ".gz" => "application/gzip",
                ".z" => "application/x-compress",
                ".img" => "application/octet-stream",
                ".iso" => "application/octet-stream",
                ".7z" => "application/x-7z-compressed",

                ".mp4" => "video/mp4",
                ".avi" => "video/x-msvideo",
                ".mkv" => "video/x-matroska",
                ".3gp" => "video/3gpp",
                ".mov" => "video/quicktime",
                ".wmv" => "video/x-ms-wmv",
                ".webm" => "video/webm",
                ".avchd" => "video/vnd.dlna.mpeg-tts",

                ".mp3" => "audio/mpeg",
                ".wma" => "audio/x-ms-wma",
                ".wav" => "audio/wav",
                ".aac" => "audio/aac",
                ".midi" => "audio/midi",
                ".flac" => "audio/flac",
                ".3ga" => "audio/3gpp",
                ".au" => "audio/basic",

                ".pdf" => "application/pdf",
                ".epub" => "application/epub+zip",
                ".doc" => "application/msword",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".xls" => "application/vnd.ms-excel",
                ".djvu" => "image/vnd.djvu",
                ".txt" => "text/plain",
                ".odt" => "application/vnd.oasis.opendocument.text",

                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".tiff" => "image/tiff",
                ".ico" => "image/x-icon",
                ".bmp" => "image/bmp",

                ".exe" => "application/octet-stream",
                ".apk" => "application/vnd.android.package-archive",
                ".ipa" => "application/octet-stream",
                ".dmg" => "application/octet-stream",
                ".jar" => "application/java-archive",
                ".sql" => "application/x-sql",
                ".xml" => "application/xml",
                ".json" => "application/json",

                _ => "application/octet-stream"
            };
        }

        private string FormatFileSize(long fileSize)
        {
            return fileSize switch
            {
                < 1024 * 1024 => $"{fileSize / 1024.0:F2} KB",
                < 1024 * 1024 * 1024 => $"{fileSize / (1024.0 * 1024.0):F2} MB",
                _ => $"{fileSize / (1024.0 * 1024.0 * 1024.0):F2} GB"
            };
        }


    }
}
