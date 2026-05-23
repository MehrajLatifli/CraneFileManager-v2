using CraneFileManager.Application.Cache.RedisCachePatterns.Abstract;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace CraneFileManager.Application.RedisCachePatterns.Concrete
{
    public class FileTrashCanCacheService<T> : IFileTrashCanCacheService<T>
    {
        private readonly IDatabase _database;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5); private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public FileTrashCanCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer?.GetDatabase()
                        ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        }

        #region FileTrashCanCache

        private string GenerateFileTrashCanKey(string name) => $"FileTrashCan:{name}";

        public async Task<string> GetFileTrashCanKey(string name)
        {
            return GenerateFileTrashCanKey(name);
        }

        public async Task<List<T>> GetAllFileTrashCans()
        {
            await _semaphore.WaitAsync(); try
            {
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: "FileTrashCan:*").ToList();
                var FileTrashCans = new List<T>();

                foreach (var key in keys)
                {
                    var value = await _database.StringGetAsync(key);
                    if (value.HasValue)
                    {
                        var FileTrashCan = JsonConvert.DeserializeObject<T>(value.ToString());
                        FileTrashCans.Add(FileTrashCan);
                    }
                }

                return FileTrashCans;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<T>> GetAllFileTrashCansById(string Id)
        {
            await _semaphore.WaitAsync();
            try
            {
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: "FileTrashCan:*").ToList();
                var FileTrashCans = new List<T>();

                foreach (var key in keys)
                {
                    var FileTrashCanName = key.ToString().Split(':').Last();
                    if (FileTrashCanName.StartsWith(Id, StringComparison.OrdinalIgnoreCase))
                    {
                        var value = await _database.StringGetAsync(key);
                        if (value.HasValue)
                        {
                            var FileTrashCan = JsonConvert.DeserializeObject<T>(value.ToString());
                            FileTrashCans.Add(FileTrashCan);
                        }
                    }
                }

                return FileTrashCans;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteAllFileTrashCansByFile(string fileId)
        {
            await _semaphore.WaitAsync();
            try
            {
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: "FileTrashCan:*").ToList();

                foreach (var key in keys)
                {
                    var FileTrashCanName = key.ToString().Split(':').Last();
                    if (FileTrashCanName.StartsWith(fileId, StringComparison.OrdinalIgnoreCase))
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

        public async Task<T> GetFileTrashCan(string name)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateFileTrashCanKey(name);
                var value = await _database.StringGetAsync(key);
                return value.HasValue ? JsonConvert.DeserializeObject<T>(value.ToString()) : default;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AddFileTrashCan(string name, T item)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateFileTrashCanKey(name);
                var value = JsonConvert.SerializeObject(item);
                await _database.StringSetAsync(key, value, CacheDuration);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task UpdateFileTrashCan(string name, T item)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateFileTrashCanKey(name);
                var value = JsonConvert.SerializeObject(item);
                await _database.StringSetAsync(key, value, CacheDuration);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteFileTrashCan(string name)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateFileTrashCanKey(name);
                await _database.KeyDeleteAsync(key);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteAllFileTrashCans()
        {
            await _semaphore.WaitAsync();
            try
            {
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: "FileTrashCan:*").ToList();

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

        #endregion
    }
}
