using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using CraneFileManager.Persistence.Repositories.Concrete;

namespace CraneFileManager.Persistence.Repositories.Custom
{
    public class FileShareReadRepository : ReadRepository<Domain.Entities.Models.FileShare>, IFileShareReadRepository
    {
        public FileShareReadRepository(CraneFileManagerContext context) : base(context)
        {
        }
    }
}
