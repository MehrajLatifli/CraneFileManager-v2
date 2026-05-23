using CraneFileManager.Application.Cache.RedisCachePatterns.Abstract;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CraneFileManager.Application.RedisCachePatterns.Concrete
{
    public class AuthCacheService<T> : IAuthCacheService<T>
    {
        private readonly IDatabase _database;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public AuthCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer?.GetDatabase()
                        ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        }

        #region ProfileCache

        private string GenerateProfileKey(string username) => $"Profile:{username}";

        public async Task<string> GetProfileKey(string username)
        {
            return GenerateProfileKey(username);
        }

        public async Task<T> GetProfile(string username)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateProfileKey(username);
                var value = await _database.StringGetAsync(key);
                return value.HasValue ? Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value.ToString()) : default;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AddProfile(string username, T item)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateProfileKey(username);
                var value = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                await _database.StringSetAsync(key, value, CacheDuration);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        #endregion

        #region UserCache

        private string GenerateUserKey(string username) => $"User:{username}";

        public async Task<string> GetUserKey(string username)
        {
            return GenerateUserKey(username);
        }

        public async Task<List<T>> GetAllUsers()
        {
            await _semaphore.WaitAsync();
            try
            {
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: "User:*");
                var users = new List<T>();

                foreach (var key in keys)
                {
                    var value = await _database.StringGetAsync(key);
                    if (value.HasValue)
                    {
                        var user = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value.ToString());
                        users.Add(user);
                    }
                }

                return users;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<T> GetUser(string username)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateUserKey(username);
                var value = await _database.StringGetAsync(key);
                return value.HasValue ? Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value.ToString()) : default;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AddUser(string username, T item)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateUserKey(username);
                var value = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                await _database.StringSetAsync(key, value, CacheDuration);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task UpdateUser(string username, T item)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateUserKey(username);
                var value = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                await _database.StringSetAsync(key, value, CacheDuration);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteUser(string username)
        {
            await _semaphore.WaitAsync();
            try
            {
                var key = GenerateUserKey(username);
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
