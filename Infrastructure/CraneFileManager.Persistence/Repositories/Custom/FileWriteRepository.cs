using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using CraneFileManager.Persistence.Repositories.Concrete;

namespace CraneFileManager.Persistence.Repositories.Custom
{
    public class FileWriteRepository : WriteRepository<Domain.Entities.Models.File>, IFileWriteRepository
    {
        public FileWriteRepository(CraneFileManagerContext context) : base(context)
        {
        }
    }
}
