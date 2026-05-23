using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Cache.RedisCachePatterns.Abstract
{
    public interface IFileCacheService<T>
    {
        public Task<string> GetFileKey(string name);
        public Task<List<T>> GetAllFiles();
        public Task<List<T>> GetAllFilesByUser(string username);
        public Task<T> GetFile(string name);
        public Task AddFile(string name, T item);
        public Task UpdateFile(string name, T item);
        public Task DeleteFile(string name);
        public Task DeleteAllFiles();
        public Task DeleteAllFilesByUser(string username);
        public Task<List<T>> GetAllFilesInTrashCanByUser(string username);

        public Task AddFileInTrashCanByUser(string name, T item);

        public Task DeleteAllFilesInTrashCanByUser(string username);

    }

}
