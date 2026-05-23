using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using CraneFileManager.Persistence.Repositories.Concrete;

namespace CraneFileManager.Persistence.Repositories.Custom
{
    public class FileReadRepository : ReadRepository<Domain.Entities.Models.File>, IFileReadRepository
    {
        public FileReadRepository(CraneFileManagerContext context) : base(context)
        {
        }
    }
}
