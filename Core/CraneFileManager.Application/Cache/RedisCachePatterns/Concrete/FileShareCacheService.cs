using CraneFileManager.Application.Cache.RedisCachePatterns.Abstract;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace CraneFileManager.Application.RedisCachePatterns.Concrete
{
    public class FileShareCacheService<T> : IFileShareCacheService<T>
    {
        private readonly IDatabase _database;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public FileShareCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer?.GetDatabase()
                        ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        }

        #region FileShareCache

        private string GenerateFileShareKey(string Id) => $"FileShare:{Id}";

        public async Task<string> GetFileShareKey(string Id)
        {
            return GenerateFileShareKey(Id);
        }

        public async Task<List<T>> GetAllFileShares()
        {
            await _semaphore.WaitAsync();
            try
            {
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: "FileShare:*");
                var fileShares = new List<T>();

                foreach (var key in keys)
                {
                    var value = await _database.StringGetAsync(key);
                    if (value.HasValue)
                    {
                        var fileShare = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value.ToString());
                        fileShares.Add(fileShare);
                    }
                }

                return fileShares;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<T> GetFileShare(string Id)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateFileShareKey(Id);
                var value = await _database.StringGetAsync(key);
                return value.HasValue ? Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value.ToString()) : default;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AddFileShare(string Id, T item)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateFileShareKey(Id);
                var value = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                await _database.StringSetAsync(key, value, CacheDuration);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task UpdateFileShare(string Id, T item)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateFileShareKey(Id);
                var value = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                await _database.StringSetAsync(key, value, CacheDuration);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteFileShare(string Id)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateFileShareKey(Id);
                await _database.KeyDeleteAsync(key);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        #endregion
    }
}
