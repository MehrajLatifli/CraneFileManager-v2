using CraneFileManager.Application.Repositories.Abstract;
using CraneFileManager.Domain.Entities.Base;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Persistence.Repositories.Concrete
{
    public class ReadRepository<T> : IReadRepository<T> where T : BaseEntity
    {

        private readonly CraneFileManagerContext _context;

        public ReadRepository(CraneFileManagerContext context)
        {
            _context = context;
        }

        public DbSet<T> Table => _context.Set<T>();

        public IQueryable<T> GetAll(bool tracking = true)
        {
            var query = Table.AsQueryable();

            if (!tracking)
            {
                query = Table.AsNoTracking();
            }

            return query;
        }

        public async Task<T> GetByIdAsync(Guid id, bool tracking = true)
        {


            var query = Table.AsQueryable();
            if (!tracking)
            {
                query = Table.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(data => data.Id == id);
        }

        public async Task<T> GetSingleAsync(Expression<Func<T, bool>> method, bool tracking = true)
        {
            var query = Table.AsQueryable();

            if (!tracking)
            {
                query = Table.AsNoTracking();
            }


            return await query.FirstOrDefaultAsync(method);
        }

        public IQueryable<T> GetWhere(Expression<Func<T, bool>> method, bool tracking = true)
        {

            var query = Table.Where(method);

            if (!tracking)
            {
                query = Table.AsNoTracking();
            }

            return query;
        }
    }
}
