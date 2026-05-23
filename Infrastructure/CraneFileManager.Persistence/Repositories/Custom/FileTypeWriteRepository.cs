using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using CraneFileManager.Persistence.Repositories.Concrete;

namespace CraneFileManager.Persistence.Repositories.Custom
{
    public class FileTypeWriteRepository : WriteRepository<Domain.Entities.Models.FileType>, IFileTypeWriteRepository
    {
        public FileTypeWriteRepository(CraneFileManagerContext context) : base(context)
        {
        }
    }
}
