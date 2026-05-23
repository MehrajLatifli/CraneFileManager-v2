using CraneFileManager.Application.Repositories.Abstract;
using CraneFileManager.Domain.Entities.Base;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CraneFileManager.Persistence.Repositories.Concrete
{
    public class WriteRepository<T> : IWriteRepository<T> where T : BaseEntity
    {

        private readonly CraneFileManagerContext _context;

        public WriteRepository(CraneFileManagerContext context)
        {
            _context = context;
        }

        public DbSet<T> Table => _context.Set<T>();

        public async Task<bool> AddAsync(T entity)
        {

            EntityEntry<T> entityEntry = await Table.AddAsync(entity);
            return entityEntry.State == EntityState.Added;


        }

        public async Task<bool> AddRangeAsync(List<T> entities)
        {
            await Table.AddRangeAsync(entities);

            return true;
        }

        public bool Remove(T entity)
        {
            EntityEntry<T> entityEntry = Table.Remove(entity);

            return entityEntry.State == EntityState.Deleted;
        }

        public async Task<bool> RemoveByIdAsync(Guid id)
        {
            T model = await Table.FirstOrDefaultAsync(data => data.Id == id);

            return Remove(model);
        }

        public bool RemoveRange(List<T> entities)
        {
            Table.RemoveRange(entities);

            return true;
        }


        public bool Update(T entity)
        {
            EntityEntry<T> entityEntry = Table.Update(entity);

            return entityEntry.State == EntityState.Modified;
        }


        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }


    }
}
