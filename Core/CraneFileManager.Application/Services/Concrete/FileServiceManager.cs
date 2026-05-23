using AutoMapper;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using CraneFileManager.Application.Cache.RedisCachePatterns.Abstract;
using CraneFileManager.Application.Mapper.DTO.AuthDTO;
using CraneFileManager.Application.Mapper.DTO.FileDTO;
using CraneFileManager.Application.Mapper.DTO.FileShareDTO;
using CraneFileManager.Application.Mapper.DTO.UserDTO;
using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Application.Services.Abstract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CraneFileManager.Domain.Entities.Models;
using CraneFileManager.Application.Mapper.DTO.FileTypeDTO;
using CraneFileManager.Application.Mapper.DTO.UserFileDTO;
using CraneFileManager.Domain.Entities.IdentityAuth;
using CraneFileManager.Application.Exceptions;
using CraneFileManager.Application.MessageBroker.RabbitMQ.Custom;
using Microsoft.AspNetCore.Http;
using CraneFileManager.Application.MessageBroker.RabbitMQ.Abstract;
using Newtonsoft.Json;
using Azure.Storage.Blobs.Specialized;
using NodaTime;
using System.IO;
using CraneFileManager.Application.Mapper.DTO.FileTrashCanDTO;
using MongoDB.Driver.Linq;
using System.Globalization;
using k8s.KubeConfigModels;

namespace CraneFileManager.Application.Services.Concrete
{
    public class FileServiceManager : IFileService
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationServiceManager> _logger;

        private readonly IFileCacheService<FileDTOforGetandGetAll> _cacheServiceFileDTOforGetandGetAll;
        private readonly IFileCacheService<FileDTOforUpdate> _cacheServiceFileDTOforUpdate;
        private readonly IFileCacheService<FileDTOforCreate> _cacheServiceFileDTOforCreate;

        private readonly IFileTrashCanCacheService<FileTrashCanDTOforGetandGetAll> _cacheServiceFileTrashCanDTOforGetandGetAll;
        private readonly IFileTrashCanCacheService<FileTrashCanDTOforUpdate> _cacheServiceFileTrashCanDTOforUpdate;
        private readonly IFileTrashCanCacheService<FileTrashCanDTOforCreate> _cacheServiceFileTrashCanDTOforCreate;


        private readonly IFileShareCacheService<FileShareDTOforGetandGetAll> _cacheServiceFileShareDTOforGetandGetAll;
        private readonly IFileShareCacheService<FileShareDTOforUpdate> _cacheServiceFileShareDTOforUpdate;
        private readonly IFileShareCacheService<FileShareDTOforCreate> _cacheServiceFileShareDTOforCreate;

        private readonly IAuthCacheService<UserDTOforGetandGetAll> _cacheServiceUserDTOforGetandGetAll;
        private readonly IAuthCacheService<UserDTOforUpdate> _cacheServiceUserDTOforUpdate;
        private readonly IAuthCacheService<UserDTOforCreate> _cacheServiceUserDTOforCreate;
        private readonly IAuthCacheService<GetUserDTOModel> _cacheServiceGetAuthDTOModel;

        private readonly IFileWriteRepository _fileWriteRepository;
        private readonly IFileReadRepository _fileReadRepository;
        private readonly IFileTrashCanWriteRepository _fileTrashCanWriteRepository;
        private readonly IFileTrashCanReadRepository _fileTrashCanReadRepository;
        private readonly IFileShareWriteRepository _fileShareWriteRepository;
        private readonly IFileShareReadRepository _fileShareReadRepository;
        private readonly IFileTypeWriteRepository _fileTypeWriteRepository;
        private readonly IFileTypeReadRepository _fileTypeReadRepository;
        private readonly IUserFileWriteRepository _userFileWriteRepository;
        private readonly IUserFileReadRepository _userFileReadRepository;
        private readonly IUserWriteRepository _userWriteRepository;
        private readonly IUserReadRepository _userReadRepository;




        public FileServiceManager(IConfiguration configuration, IMapper mapper, ILogger<NotificationServiceManager> logger, IFileCacheService<FileDTOforGetandGetAll> cacheServiceFileDTOforGetandGetAll, IFileCacheService<FileDTOforUpdate> cacheServiceFileDTOforUpdate, IFileCacheService<FileDTOforCreate> cacheServiceFileDTOforCreate, IFileTrashCanCacheService<FileTrashCanDTOforGetandGetAll> cacheServiceFileTrashCanDTOforGetandGetAll, IFileTrashCanCacheService<FileTrashCanDTOforUpdate> cacheServiceFileTrashCanDTOforUpdate, IFileTrashCanCacheService<FileTrashCanDTOforCreate> cacheServiceFileTrashCanDTOforCreate, IFileShareCacheService<FileShareDTOforGetandGetAll> cacheServiceFileShareDTOforGetandGetAll, IFileShareCacheService<FileShareDTOforUpdate> cacheServiceFileShareDTOforUpdate, IFileShareCacheService<FileShareDTOforCreate> cacheServiceFileShareDTOforCreate, IAuthCacheService<UserDTOforGetandGetAll> cacheServiceUserDTOforGetandGetAll, IAuthCacheService<UserDTOforUpdate> cacheServiceUserDTOforUpdate, IAuthCacheService<UserDTOforCreate> cacheServiceUserDTOforCreate, IAuthCacheService<GetUserDTOModel> cacheServiceGetAuthDTOModel, IFileWriteRepository fileWriteRepository, IFileReadRepository fileReadRepository, IFileTrashCanWriteRepository fileTrashCanWriteRepository, IFileTrashCanReadRepository fileTrashCanReadRepository, IFileShareWriteRepository fileShareWriteRepository, IFileShareReadRepository fileShareReadRepository, IFileTypeWriteRepository fileTypeWriteRepository, IFileTypeReadRepository fileTypeReadRepository, IUserFileWriteRepository userFileWriteRepository, IUserFileReadRepository userFileReadRepository, IUserWriteRepository userWriteRepository, IUserReadRepository userReadRepository)
        {
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
            _cacheServiceFileDTOforGetandGetAll = cacheServiceFileDTOforGetandGetAll;
            _cacheServiceFileDTOforUpdate = cacheServiceFileDTOforUpdate;
            _cacheServiceFileDTOforCreate = cacheServiceFileDTOforCreate;
            _cacheServiceFileTrashCanDTOforGetandGetAll = cacheServiceFileTrashCanDTOforGetandGetAll;
            _cacheServiceFileTrashCanDTOforUpdate = cacheServiceFileTrashCanDTOforUpdate;
            _cacheServiceFileTrashCanDTOforCreate = cacheServiceFileTrashCanDTOforCreate;
            _cacheServiceFileShareDTOforGetandGetAll = cacheServiceFileShareDTOforGetandGetAll;
            _cacheServiceFileShareDTOforUpdate = cacheServiceFileShareDTOforUpdate;
            _cacheServiceFileShareDTOforCreate = cacheServiceFileShareDTOforCreate;
            _cacheServiceUserDTOforGetandGetAll = cacheServiceUserDTOforGetandGetAll;
            _cacheServiceUserDTOforUpdate = cacheServiceUserDTOforUpdate;
            _cacheServiceUserDTOforCreate = cacheServiceUserDTOforCreate;
            _cacheServiceGetAuthDTOModel = cacheServiceGetAuthDTOModel;
            _fileWriteRepository = fileWriteRepository;
            _fileReadRepository = fileReadRepository;
            _fileTrashCanWriteRepository = fileTrashCanWriteRepository;
            _fileTrashCanReadRepository = fileTrashCanReadRepository;
            _fileShareWriteRepository = fileShareWriteRepository;
            _fileShareReadRepository = fileShareReadRepository;
            _fileTypeWriteRepository = fileTypeWriteRepository;
            _fileTypeReadRepository = fileTypeReadRepository;
            _userFileWriteRepository = userFileWriteRepository;
            _userFileReadRepository = userFileReadRepository;
            _userWriteRepository = userWriteRepository;
            _userReadRepository = userReadRepository;
        }



        public async Task CreateFile(string ConnectionStringAzure, string CurrentUser, string filename, string filedisplayname, long fileSize, string filePath)
        {
            if (!_mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false)).Any(i => string.IsNullOrEmpty(i.RefreshToken) && i.Username == CurrentUser))
            {
                var currentUser = CurrentUser;
                var users = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false));


                string path = filePath;

                string fileNameWithExtension = Path.GetFileName(path);
                string fileType = Path.GetExtension(fileNameWithExtension).TrimStart('.');
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileNameWithExtension);


                long fileSizeInBytes = fileSize;
                string formattedFileSize = FormatFileSize(fileSizeInBytes);




                System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();
                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime localTime = localZone.ToLocalTime(DateTime.UtcNow);


                var mapfilTypes = _mapper.Map<List<CraneFileManager.Domain.Entities.Models.FileType>>(_fileTypeReadRepository.GetAll(false));

                if (!mapfilTypes.Any(i => i.Type == fileType))
                {
                    var fileType_ = _mapper.Map<CraneFileManager.Domain.Entities.Models.FileType>(new FileTypeDTOforCreate
                    {
                        Type = fileType,
                        CreatedDate = localTime,
                        UpdatedDate = null,
                    });

                    await _fileTypeWriteRepository.AddAsync(fileType_);
                    var fileTypeResult = await _fileTypeWriteRepository.SaveAsync();

                    if (fileTypeResult == -1)
                    {
                        throw new InvalidOperationException("Failed to create the FileType.");
                    }
                }

                var file = _mapper.Map<CraneFileManager.Domain.Entities.Models.File>(new FileDTOforCreate
                {
                    OrginalName = fileNameWithExtension,
                    DisplayName = Path.GetFileName(filedisplayname),
                    Description = $"File name: {fileNameWithoutExtension}; File extension: {fileType}; File size: {formattedFileSize}; File created date {localTime}; File updated date {null}",
                    Size = formattedFileSize,
                    Path = $"{path}",
                    IsRemove = false,
                    CreatedDate = localTime,
                    UpdatedDate = null,
                    FileTypeId = _mapper.Map<List<CraneFileManager.Domain.Entities.Models.FileType>>(_fileTypeReadRepository.GetAll(false)).FirstOrDefault(i => i.Type == fileType).Id,
                });

                await _fileWriteRepository.AddAsync(file);
                var fileResult = await _fileWriteRepository.SaveAsync();

                if (fileResult == -1)
                {
                    throw new InvalidOperationException("Failed to create the FileType.");
                }

                var userFile = new UserFile
                {
                    CreatedDate = localTime,
                    FileId = file.Id,
                    UserId = users.FirstOrDefault(u => u.Username == currentUser)?.Id ?? throw new UnauthorizedAccessException("User not found.")
                };

                await _userFileWriteRepository.AddAsync(userFile);
                var userFileResult = await _userFileWriteRepository.SaveAsync();

                if (userFileResult == -1)
                {
                    throw new InvalidOperationException("Failed to create the FileType.");
                }

                var fileToAdd = _mapper.Map<FileDTOforCreate>(file);
                _cacheServiceFileDTOforCreate.AddFile(file.OrginalName, fileToAdd);
            }
            else
            {
                throw new UnauthorizedException("Current user is not authenticated.");
            }
        }



        public async Task AddTrashCan(Guid Id, string ConnectionStringAzure, string CurrentUser)
        {
            
                // Check if the user is authenticated
                var users = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false));
                var user = users.FirstOrDefault(u => u.Username == CurrentUser);
                if (user == null || string.IsNullOrEmpty(user.RefreshToken))
                {
                    throw new UnauthorizedException("User is not authenticated.");
                }

                // Retrieve files, user-files relationships, and file trash can entries
                var files = _mapper.Map<List<FileDTOforGetandGetAll>>(_fileReadRepository.GetAll(false));
                var userFiles = _mapper.Map<List<UserFileDTOforGetandGetAll>>(_userFileReadRepository.GetAll(false));

                // Filter files that are not marked for removal
                var filteredFiles = (from u in users
                                     join uf in userFiles on u.Id equals uf.UserId
                                     join f in files on uf.FileId equals f.Id
                                     where u.Username == CurrentUser && f.IsRemove == false
                                     select f).OrderBy(o => o.CreatedDate).ToList();

                // Ensure the file to update exists
                var fileToUpdate = filteredFiles.FirstOrDefault(i => i.Id == Id);
                if (fileToUpdate == null)
                {
                    throw new NotFoundException("File not found.");
                }

                // Get file type (ensure it exists)
                var filetype = _fileTypeReadRepository.GetAll(false).FirstOrDefault(i => i.Id == fileToUpdate.FileTypeId);
                if (filetype == null)
                {
                    throw new NotFoundException("File type not found.");
                }

                // Get the current time in the local time zone
                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime localTime = localZone.ToLocalTime(DateTime.UtcNow);

                // Create the file entity to be updated (mark as removed)
                var fileEntity = new Domain.Entities.Models.File
                {
                    Id = Id,
                    DisplayName = $"{fileToUpdate.DisplayName}",
                    UpdatedDate = localTime,
                    Size = fileToUpdate.Size,
                    CreatedDate = fileToUpdate.CreatedDate,
                    Description = $"File name: {fileToUpdate.DisplayName}; File extension: {filetype.Type}; File size: {fileToUpdate.Size}; File created date {fileToUpdate.CreatedDate}; File updated date {localTime}",
                    OrginalName = fileToUpdate.OrginalName,
                    Path = fileToUpdate.Path,
                    IsRemove = true,
                    FileTypeId = fileToUpdate.FileTypeId,
                };

                // Update the file in the repository
                _fileWriteRepository.Update(fileEntity);
                var fileResult = await _fileWriteRepository.SaveAsync();
                if (fileResult == -1)
                {
                    throw new InvalidOperationException("Failed to update the file.");
                }


                var fileTrashCanEntity = _fileTrashCanReadRepository.GetAll(false).FirstOrDefault(i => i.FileId == Id);

                if (fileTrashCanEntity == null)
                {



                    // Create the FileTrashCan entry
                    fileTrashCanEntity = new FileTrashCan()
                    {
                        ThrowTrashDate = localTime,
                        TakeofTrashDate = null,
                        FileId = Id,
                    };

                    // Add the trash can entry to the repository
                    await _fileTrashCanWriteRepository.AddAsync(fileTrashCanEntity);
                    var fileTrashCanResult = await _fileTrashCanWriteRepository.SaveAsync();
                    if (fileTrashCanResult == -1)
                    {
                        throw new InvalidOperationException("Failed to update the file trash can.");
                    }
                }
                else
                {
                    fileTrashCanEntity.ThrowTrashDate = localTime;
                    fileTrashCanEntity.TakeofTrashDate = null;

                    // Update the file trash can entry in the repository
                    _fileTrashCanWriteRepository.Update(fileTrashCanEntity);
                    var fileTrashCanResult = await _fileTrashCanWriteRepository.SaveAsync();
                    if (fileTrashCanResult == -1)
                    {
                        throw new InvalidOperationException("Failed to update the file trash can.");
                    }

                }

                // Cache check and update
                var cachedFiles = await _cacheServiceFileDTOforGetandGetAll.GetAllFilesByUser(CurrentUser);
                if (cachedFiles == null || cachedFiles.Count == 0)
                {
                    // Cache files if they don't exist
                    foreach (var item in filteredFiles)
                    {
                        await _cacheServiceFileDTOforGetandGetAll.AddFile(item.OrginalName, item);
                    }

                    // Reload the cached files after adding
                    cachedFiles = await _cacheServiceFileDTOforGetandGetAll.GetAllFilesByUser(CurrentUser);
                }

                // Ensure the file exists in the cached files
                var fromCachedFiles = cachedFiles.Where(i => i.Id == Id).ToList();
                if (fromCachedFiles == null || !fromCachedFiles.Any())
                {
                    throw new NotFoundException("File not found in cache.");
                }

                // Get the trash cans associated with the file
                var fileId = fromCachedFiles.FirstOrDefault()?.Id.ToString();
                if (string.IsNullOrEmpty(fileId))
                {
                    throw new ArgumentException("File ID is null or empty.");
                }

              
        }


        public async Task UpdateTrashCan(Guid Id, string ConnectionStringAzure, string CurrentUser)
        {
           
                // Check if the user is authenticated
                var users = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false));
                var user = users.FirstOrDefault(u => u.Username == CurrentUser);
                if (user == null || string.IsNullOrEmpty(user.RefreshToken))
                {
                    throw new UnauthorizedException("User is not authenticated.");
                }

                // Retrieve files, user-files relationships, and file trash can entries
                var files = _mapper.Map<List<FileDTOforGetandGetAll>>(_fileReadRepository.GetAll(false));
                var userFiles = _mapper.Map<List<UserFileDTOforGetandGetAll>>(_userFileReadRepository.GetAll(false));

                // Filter files that are not marked for removal
                var filteredFiles = (from u in users
                                     join uf in userFiles on u.Id equals uf.UserId
                                     join f in files on uf.FileId equals f.Id
                                     where u.Username == CurrentUser && f.IsRemove == true
                                     select f).OrderBy(o => o.CreatedDate).ToList();

                // Ensure the file to update exists
                var fileToUpdate = filteredFiles.FirstOrDefault(i => i.Id == Id);
                if (fileToUpdate == null)
                {
                    throw new NotFoundException("File not found.");
                }

                // Get file type (ensure it exists)
                var filetype = _fileTypeReadRepository.GetAll(false).FirstOrDefault(i => i.Id == fileToUpdate.FileTypeId);
                if (filetype == null)
                {
                    throw new NotFoundException("File type not found.");
                }

                // Get the current time in the local time zone
                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime localTime = localZone.ToLocalTime(DateTime.UtcNow);

                // Create the file entity to be updated (mark as removed)
                var fileEntity = new Domain.Entities.Models.File
                {
                    Id = Id,
                    DisplayName = $"{fileToUpdate.DisplayName}",
                    UpdatedDate = localTime,
                    Size = fileToUpdate.Size,
                    CreatedDate = fileToUpdate.CreatedDate,
                    Description = $"File name: {fileToUpdate.DisplayName}; File extension: {filetype.Type}; File size: {fileToUpdate.Size}; File created date {fileToUpdate.CreatedDate}; File updated date {localTime}",
                    OrginalName = fileToUpdate.OrginalName,
                    Path = fileToUpdate.Path,
                    IsRemove = false,
                    FileTypeId = fileToUpdate.FileTypeId,
                };

                // Update the file in the repository
                _fileWriteRepository.Update(fileEntity);
                var fileResult = await _fileWriteRepository.SaveAsync();
                if (fileResult == -1)
                {
                    throw new InvalidOperationException("Failed to update the file.");
                }

                // Retrieve the file trash can entity from the repository
                var fileTrashCanEntity = _fileTrashCanReadRepository.GetAll(false).FirstOrDefault(i => i.FileId == Id);
                if (fileTrashCanEntity == null)
                {
                    throw new NotFoundException("File trash can entry not found.");
                }

                // Update trash can entity (set ThrowTrashDate to null, TakeofTrashDate to current time)
                fileTrashCanEntity.ThrowTrashDate = null;
                fileTrashCanEntity.TakeofTrashDate = localTime;

                // Update the file trash can entry in the repository
                _fileTrashCanWriteRepository.Update(fileTrashCanEntity);
                var fileTrashCanResult = await _fileTrashCanWriteRepository.SaveAsync();
                if (fileTrashCanResult == -1)
                {
                    throw new InvalidOperationException("Failed to update the file trash can.");
                }

                // Cache check and update for files
                var cachedFiles = await _cacheServiceFileDTOforGetandGetAll.GetAllFilesByUser(CurrentUser);
                if (cachedFiles == null || cachedFiles.Count == 0)
                {
                    foreach (var item in filteredFiles)
                    {
                        await _cacheServiceFileDTOforGetandGetAll.AddFile(item.OrginalName, item);
                    }

                    // Reload the cached files after adding
                    cachedFiles = await _cacheServiceFileDTOforGetandGetAll.GetAllFilesByUser(CurrentUser);
                }

                // Ensure the file exists in the cached files
                var fromCachedFiles = cachedFiles.Where(i => i.Id == Id).ToList();
                if (fromCachedFiles == null || !fromCachedFiles.Any())
                {
                    throw new NotFoundException("File not found in cache.");
                }

                // Get the trash cans associated with the file
                var fileId = fromCachedFiles.FirstOrDefault()?.Id.ToString();
                if (string.IsNullOrEmpty(fileId))
                {
                    throw new ArgumentException("File ID is null or empty.");
                }

                // Try fetching cached trash cans by fileId
             
        
        }

        public async Task<List<FileDTOforGetandGetAll>> ViewFiles(string CurrentUser)
        {

            if (!_mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false)).AsEnumerable().Any(i => string.IsNullOrEmpty(i.RefreshToken) && i.Username == CurrentUser))
            {
                var users = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false));
                var files = _mapper.Map<List<FileDTOforGetandGetAll>>(_fileReadRepository.GetAll(false));
                var userFiles = _mapper.Map<List<UserFileDTOforGetandGetAll>>(_userFileReadRepository.GetAll(false));
                var fileTypes = _mapper.Map<List<FileTypeDTOforGetandGetAll>>(_fileTypeReadRepository.GetAll(false));

                var cachedUsers = await _cacheServiceUserDTOforGetandGetAll.GetAllUsers();

                if (cachedUsers.Count <= 0)
                {
                    foreach (var item in users)
                    {
                        await _cacheServiceUserDTOforGetandGetAll.AddUser(item.Username, item);
                    }
                }

                var filteredFiles = (from u in users
                                     join uf in userFiles on u.Id equals uf.UserId
                                     join f in files on uf.FileId equals f.Id
                                     join ft in fileTypes on f.FileTypeId equals ft.Id
                                     where u.Username == CurrentUser && f.IsRemove == false
                                     select new
                                     {
                                         f.Id,
                                         f.OrginalName,
                                         f.DisplayName,
                                         f.Description,
                                         f.IsRemove,
                                         f.Size,
                                         f.Path,
                                         CreatedDate = f.CreatedDate?.ToString("dd-MMM-yy HH:mm:ss"),
                                         UpdatedDate = f.UpdatedDate?.ToString("dd-MMM-yy HH:mm:ss"),
                                         FileType = ft.Type  // Only FileType, not FileTypeId
                                     })
                                    .OrderBy(o => o.CreatedDate)
                                    .ToList();

                if (filteredFiles.Count <= 0)
                {
                    throw new NotFoundException("File not found.");
                }

                var cachedFiles = await _cacheServiceFileDTOforGetandGetAll.GetAllFilesByUser(CurrentUser);
                if (cachedFiles.Count > 0)
                {
                    return cachedFiles;
                }
                else
                {
                    foreach (var item in filteredFiles)
                    {
                        // Manually parse the string back to DateTime? when creating the DTO
                        var fileDto = new FileDTOforGetandGetAll
                        {
                            Id = item.Id,
                            OrginalName = item.OrginalName,
                            DisplayName = item.DisplayName,
                            Description = item.Description,
                            IsRemove = item.IsRemove ?? false,  // Handle nullable boolean
                            Size = item.Size,
                            Path = item.Path,
                            CreatedDate = !string.IsNullOrEmpty(item.CreatedDate)
                                ? DateTime.ParseExact(item.CreatedDate, "dd-MMM-yy HH:mm:ss", CultureInfo.InvariantCulture)
                                : (DateTime?)null,  // Parse only if not null or empty
                            UpdatedDate = !string.IsNullOrEmpty(item.UpdatedDate)
                                ? DateTime.ParseExact(item.UpdatedDate, "dd-MMM-yy HH:mm:ss", CultureInfo.InvariantCulture)
                                : (DateTime?)null,  // Parse only if not null or empty
                            FileType = item.FileType ?? "Unknown"  // Default value if FileType is null
                        };


                        await _cacheServiceFileDTOforGetandGetAll.AddFile(item.OrginalName, fileDto);
                    }

                    // Fetch the updated cached files
                    cachedFiles = await _cacheServiceFileDTOforGetandGetAll.GetAllFilesByUser(CurrentUser);

                    return cachedFiles;
                }


            }
            else
            {
                throw new UnauthorizedException("Current user is not authenticated.");
            }


        }

        public async Task<List<FileDTOforGetandGetAll>> ViewFilesInTrashCan(string CurrentUser)
        {

            if (!_mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false)).AsEnumerable().Any(i => string.IsNullOrEmpty(i.RefreshToken) && i.Username == CurrentUser))
            {
                var users = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false));
                var files = _mapper.Map<List<FileDTOforGetandGetAll>>(_fileReadRepository.GetAll(false));
                var userFiles = _mapper.Map<List<UserFileDTOforGetandGetAll>>(_userFileReadRepository.GetAll(false));
                var fileTrashCans = _mapper.Map<List<FileTrashCanDTOforGetandGetAll>>(_fileTrashCanReadRepository.GetAll(false));
                var fileTypes = _mapper.Map<List<FileTypeDTOforGetandGetAll>>(_fileTypeReadRepository.GetAll(false));

                var cachedUsers = await _cacheServiceUserDTOforGetandGetAll.GetAllUsers();

                if (cachedUsers.Count <= 0)
                {
                    foreach (var item in users)
                    {
                        await _cacheServiceUserDTOforGetandGetAll.AddUser(item.Username, item);
                    }
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
                                                 CreatedDate = f.CreatedDate?.ToString("dd-MMM-yy HH:mm:ss"),
                                                 UpdatedDate = f.UpdatedDate?.ToString("dd-MMM-yy HH:mm:ss"),
                                                 FileType = ft.Type  // Only FileType, not FileTypeId
                                             }).OrderBy(o => o.CreatedDate).ToList();





                if (filteredFileTrashCans.Count <= 0)
                {
                    throw new NotFoundException("File not found.");
                }

                var cachedFiles = await _cacheServiceFileDTOforGetandGetAll.GetAllFilesInTrashCanByUser(CurrentUser);
                if (cachedFiles.Count > 0)
                {
                    return cachedFiles;
                }
                else
                {
                    foreach (var item in filteredFileTrashCans)
                    {
                        // Manually parse the string back to DateTime? when creating the DTO
                        var fileDto = new FileDTOforGetandGetAll
                        {
                            Id = item.Id,
                            OrginalName = item.OrginalName,
                            DisplayName = item.DisplayName,
                            Description = item.Description,
                            IsRemove = item.IsRemove ?? false,  // Handle nullable boolean
                            Size = item.Size,
                            Path = item.Path,
                            CreatedDate = !string.IsNullOrEmpty(item.CreatedDate)
                                ? DateTime.ParseExact(item.CreatedDate, "dd-MMM-yy HH:mm:ss", CultureInfo.InvariantCulture)
                                : (DateTime?)null,  // Parse only if not null or empty
                            UpdatedDate = !string.IsNullOrEmpty(item.UpdatedDate)
                                ? DateTime.ParseExact(item.UpdatedDate, "dd-MMM-yy HH:mm:ss", CultureInfo.InvariantCulture)
                                : (DateTime?)null,  // Parse only if not null or empty
                            FileType = item.FileType ?? "Unknown"  // Default value if FileType is null
                        };

                        await _cacheServiceFileDTOforGetandGetAll.AddFileInTrashCanByUser(item.OrginalName, fileDto);
                    }

                    // Fetch the updated cached files
                    cachedFiles = await _cacheServiceFileDTOforGetandGetAll.GetAllFilesInTrashCanByUser(CurrentUser);

                    return cachedFiles;
                }




            }
            else
            {
                throw new UnauthorizedException("Current user is not authenticated.");
            }


        }

        public async Task UpdateFile(Guid Id, string displayname, string ConnectionStringAzure, string CurrentUser)
        {
            if (!_mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false)).AsEnumerable().Any(i => string.IsNullOrEmpty(i.RefreshToken) && i.Username == CurrentUser))
            {
                var users = _mapper.Map<List<UserDTOforGetandGetAll>>(_userReadRepository.GetAll(false));
                var files = _mapper.Map<List<FileDTOforGetandGetAll>>(_fileReadRepository.GetAll(false));
                var userFiles = _mapper.Map<List<UserFileDTOforGetandGetAll>>(_userFileReadRepository.GetAll(false));

                var user = _userReadRepository.GetAll(false).FirstOrDefault(u => u.Username == CurrentUser);



                var filteredFiles = (from u in users
                                     join uf in userFiles on u.Id equals uf.UserId
                                     join f in files on uf.FileId equals f.Id
                                     where u.Username == CurrentUser && f.IsRemove == false
                                     select f).OrderBy(o => o.CreatedDate).ToList();



                if (user == null)
                {
                    throw new NotFoundException("User not found.");
                }

                var fileToUpdate = filteredFiles.FirstOrDefault(i => i.Id == Id);

                var filetype = _fileTypeReadRepository.GetAll(false).Where(i => i.Id == fileToUpdate.FileTypeId).FirstOrDefault();

                if (fileToUpdate != null)
                {

                    System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();
                    TimeZone localZone = TimeZone.CurrentTimeZone;
                    DateTime localTime = localZone.ToLocalTime(DateTime.UtcNow);

                    var fileEntity = new Domain.Entities.Models.File
                    {
                        Id = Id,
                        DisplayName = $"{displayname}.{filetype.Type}",
                        UpdatedDate = localTime,
                        Size = fileToUpdate.Size,
                        CreatedDate = fileToUpdate.CreatedDate,
                        Description = $"File name: {displayname}; File extension: {filetype.Type}; File size: {fileToUpdate.Size}; File created date {fileToUpdate.CreatedDate}; File updated date {localTime}",
                        OrginalName = fileToUpdate.OrginalName,
                        Path = fileToUpdate.Path,
                        IsRemove = false,
                        FileTypeId = fileToUpdate.FileTypeId,

                    };

                    _fileWriteRepository.Update(fileEntity);

                    var fileResult = await _fileWriteRepository.SaveAsync();

                    if (fileResult == -1)
                    {
                        throw new InvalidOperationException("Failed to update the file.");
                    }

                    var fileDTO = _mapper.Map<FileDTOforUpdate>(fileEntity);

                    var cachedFiles = await _cacheServiceFileDTOforGetandGetAll.GetAllFilesByUser(CurrentUser);
                    if (cachedFiles.Count > 0)
                    {

                    }
                    else
                    {

                        foreach (var item in filteredFiles)
                        {
                            await _cacheServiceFileDTOforGetandGetAll.AddFile(item.OrginalName, item);
                        }

                        cachedFiles = await _cacheServiceFileDTOforGetandGetAll.GetAllFilesByUser(CurrentUser);


                    }
                }
                else
                {
                    throw new NotFoundException("File not found.");
                }
            }
            else
            {
                throw new UnauthorizedException("User is not authenticated.");
            }
        }

        public Task CreateFileShare(FileShareDTOforCreate model, ClaimsPrincipal claimsPrincipal)
        {
            throw new NotImplementedException();
        }
        public Task DeleteFileShare(Guid Id, ClaimsPrincipal claimsPrincipal)
        {
            throw new NotImplementedException();
        }

        public Task<FileDTOforGetandGetAll> GetFileById(Guid Id, ClaimsPrincipal claimsPrincipal)
        {
            throw new NotImplementedException();
        }

        public Task GetFileLocation(Guid Id, ClaimsPrincipal claimsPrincipal)
        {
            throw new NotImplementedException();
        }


        public Task<FileShareDTOforGetandGetAll> GetFileShareById(Guid Id, ClaimsPrincipal claimsPrincipal)
        {
            throw new NotImplementedException();
        }

        public Task<List<FileShareDTOforGetandGetAll>> GetFileShares(ClaimsPrincipal claimsPrincipal)
        {
            throw new NotImplementedException();
        }

        public Task SupportedFileTypes(ClaimsPrincipal claimsPrincipal)
        {
            throw new NotImplementedException();
        }

 
        public Task UpdateFileShare(FileShareDTOforUpdate model, ClaimsPrincipal claimsPrincipal)
        {
            throw new NotImplementedException();
        }

        private string GetAzureConnectionString(string connectionStringAzure)
        {
            string azuriteConnectionString = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_AZURE_STORAGE_CONNECTION_STRING");
            return !string.IsNullOrEmpty(azuriteConnectionString) ? azuriteConnectionString : connectionStringAzure;
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
