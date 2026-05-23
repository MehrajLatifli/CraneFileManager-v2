using CraneFileManager.Application.Cache.RedisCachePatterns.Abstract;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CraneFileManager.Application.RedisCachePatterns.Concrete
{
    public class FileCacheService<T> : IFileCacheService<T>
    {
        private readonly IDatabase _database;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5); private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public FileCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer?.GetDatabase()
                        ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        }

        #region FileCache

        private string GenerateFileKey(string name) => $"File:{name}";

        public async Task<string> GetFileKey(string name)
        {
            return GenerateFileKey(name);
        }

        private string GenerateFileInTrashCanByUserKey(string name) => $"FilesInTrashCanByUser:{name}";

        public async Task<string> GetFileInTrashCanByUsereKey(string name)
        {
            return GenerateFileInTrashCanByUserKey(name);
        }

        public async Task<List<T>> GetAllFiles()
        {
            await _semaphore.WaitAsync(); try
            {
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: "File:*").ToList();
                var files = new List<T>();

                foreach (var key in keys)
                {
                    var value = await _database.StringGetAsync(key);
                    if (value.HasValue)
                    {
                        var file = JsonConvert.DeserializeObject<T>(value.ToString());
                        files.Add(file);
                    }
                }

                return files;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<T>> GetAllFilesByUser(string username)
        {
            await _semaphore.WaitAsync();
            try
            {
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: "File:*").ToList();
                var files = new List<T>();

                foreach (var key in keys)
                {
                    var fileName = key.ToString().Split(':').Last();
                    if (fileName.StartsWith(username, StringComparison.OrdinalIgnoreCase))
                    {
                        var value = await _database.StringGetAsync(key);
                        if (value.HasValue)
                        {
                            var file = JsonConvert.DeserializeObject<T>(value.ToString());
                            files.Add(file);
                        }
                    }
                }

                return files;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteAllFilesByUser(string username)
        {
            await _semaphore.WaitAsync();
            try
            {
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: "File:*").ToList();

                foreach (var key in keys)
                {
                    var fileName = key.ToString().Split(':').Last();
                    if (fileName.StartsWith(username, StringComparison.OrdinalIgnoreCase))
                    {
                        await _database.KeyDeleteAsync(key);
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<T> GetFile(string name)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateFileKey(name);
                var value = await _database.StringGetAsync(key);
                return value.HasValue ? JsonConvert.DeserializeObject<T>(value.ToString()) : default;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AddFile(string name, T item)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateFileKey(name);
                var value = JsonConvert.SerializeObject(item);
                await _database.StringSetAsync(key, value, CacheDuration);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task UpdateFile(string name, T item)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateFileKey(name);
                var value = JsonConvert.SerializeObject(item);
                await _database.StringSetAsync(key, value, CacheDuration);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteFile(string name)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateFileKey(name);
                await _database.KeyDeleteAsync(key);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteAllFiles()
        {
            await _semaphore.WaitAsync();
            try
            {
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: "File:*").ToList();

                foreach (var key in keys)
                {
                    await _database.KeyDeleteAsync(key);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }



        public async Task<List<T>> GetAllFilesInTrashCanByUser(string username)
        {
            await _semaphore.WaitAsync();
            try
            {
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: "FilesInTrashCanByUser:*").ToList();
                var files = new List<T>();

                foreach (var key in keys)
                {
                    var fileName = key.ToString().Split(':').Last();
                    if (fileName.StartsWith(username, StringComparison.OrdinalIgnoreCase))
                    {
                        var value = await _database.StringGetAsync(key);
                        if (value.HasValue)
                        {
                            var file = JsonConvert.DeserializeObject<T>(value.ToString());
                            files.Add(file);
                        }
                    }
                }

                return files;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AddFileInTrashCanByUser(string name, T item)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateFileInTrashCanByUserKey(name);
                var value = JsonConvert.SerializeObject(item);
                await _database.StringSetAsync(key, value, CacheDuration);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteAllFilesInTrashCanByUser(string username)
        {
            await _semaphore.WaitAsync();
            try
            {
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: "FilesInTrashCanByUser:*").ToList();

                foreach (var key in keys)
                {
                    var fileName = key.ToString().Split(':').Last();
                    if (fileName.StartsWith(username, StringComparison.OrdinalIgnoreCase))
                    {
                        await _database.KeyDeleteAsync(key);
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        #endregion
    }
}
