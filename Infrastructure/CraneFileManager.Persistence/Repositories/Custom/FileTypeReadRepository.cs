using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using CraneFileManager.Persistence.Repositories.Concrete;

namespace CraneFileManager.Persistence.Repositories.Custom
{
    public class FileTypeReadRepository : ReadRepository<Domain.Entities.Models.FileType>, IFileTypeReadRepository
    {
        public FileTypeReadRepository(CraneFileManagerContext context) : base(context)
        {
        }
    }
}
