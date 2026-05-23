using CraneFileManager.Application.Mapper.DTO.AuthDTO;
using CraneFileManager.Application.Mapper.DTO.FileDTO;
using CraneFileManager.Application.Mapper.DTO.FileShareDTO;
using CraneFileManager.Application.MessageBroker.RabbitMQ.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Services.Abstract
{
    public interface IFileService
    {
        #region File service

        public Task CreateFile( string ConnectionStringAzure, string CurrentUser, string filename, string filedisplayname, long filesize, string filePath);
        public Task UpdateFile(Guid Id, string displayname, string ConnectionStringAzure, string CurrentUser);
        public Task AddTrashCan(Guid Id, string ConnectionStringAzure, string CurrentUser);
        public Task UpdateTrashCan(Guid Id, string ConnectionStringAzure, string CurrentUser);
        public Task<List<FileDTOforGetandGetAll>> ViewFiles(string CurrentUser);
        public Task<List<FileDTOforGetandGetAll>> ViewFilesInTrashCan(string CurrentUser);
        public Task<FileDTOforGetandGetAll> GetFileById(Guid Id, ClaimsPrincipal claimsPrincipal);


        public Task CreateFileShare(FileShareDTOforCreate model, ClaimsPrincipal claimsPrincipal);
        public Task UpdateFileShare(FileShareDTOforUpdate model, ClaimsPrincipal claimsPrincipal);
        public Task DeleteFileShare(Guid Id, ClaimsPrincipal claimsPrincipal);
        public Task<List<FileShareDTOforGetandGetAll>>  GetFileShares(ClaimsPrincipal claimsPrincipal);
        public Task<FileShareDTOforGetandGetAll> GetFileShareById(Guid Id, ClaimsPrincipal claimsPrincipal);


        public Task SupportedFileTypes(ClaimsPrincipal claimsPrincipal);
        public Task GetFileLocation(Guid Id, ClaimsPrincipal claimsPrincipal);


        #endregion

    }
}
