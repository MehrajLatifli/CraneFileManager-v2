namespace CraneFileManager.Application.Cache.RedisCachePatterns.Abstract
{
    public interface IFileTrashCanCacheService<T>
    {
        public Task<string> GetFileTrashCanKey(string name);
        public Task<List<T>> GetAllFileTrashCans();
        public Task<List<T>> GetAllFileTrashCansById(string id);
        public Task<T> GetFileTrashCan(string name);
        public Task AddFileTrashCan(string name, T item);
        public Task UpdateFileTrashCan(string name, T item);
        public Task DeleteFileTrashCan(string name);
        public Task DeleteAllFileTrashCans();
        public Task DeleteAllFileTrashCansByFile(string fileid);


    }

}
