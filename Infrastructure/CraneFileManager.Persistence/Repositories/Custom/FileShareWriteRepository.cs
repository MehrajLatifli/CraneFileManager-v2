using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using CraneFileManager.Persistence.Repositories.Concrete;

namespace CraneFileManager.Persistence.Repositories.Custom
{
    public class FileShareWriteRepository : WriteRepository<Domain.Entities.Models.FileShare>, IFileShareWriteRepository
    {
        public FileShareWriteRepository(CraneFileManagerContext context) : base(context)
        {
        }
    }
}
