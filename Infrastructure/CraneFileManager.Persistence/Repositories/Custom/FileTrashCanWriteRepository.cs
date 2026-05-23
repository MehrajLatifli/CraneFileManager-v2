using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using CraneFileManager.Persistence.Repositories.Concrete;

namespace CraneFileManager.Persistence.Repositories.Custom
{
    public class FileTrashCanWriteRepository : WriteRepository<Domain.Entities.Models.FileTrashCan>, IFileTrashCanWriteRepository
    {
        public FileTrashCanWriteRepository(CraneFileManagerContext context) : base(context)
        {
        }
    }
}
