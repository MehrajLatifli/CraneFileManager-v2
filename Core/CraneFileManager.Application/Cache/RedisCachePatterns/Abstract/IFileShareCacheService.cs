namespace CraneFileManager.Application.Cache.RedisCachePatterns.Abstract
{
    public interface IFileShareCacheService<T>
    {
        public Task<string> GetFileShareKey(string Id);
        public Task<List<T>> GetAllFileShares();
        public Task<T> GetFileShare(string Id);
        public Task AddFileShare(string Id, T item);
        public Task UpdateFileShare(string Id, T item);
        public Task DeleteFileShare(string Id);
    }

}
