using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using CraneFileManager.Persistence.Repositories.Concrete;

namespace CraneFileManager.Persistence.Repositories.Custom
{
    public class FileTrashCanReadRepository : ReadRepository<Domain.Entities.Models.FileTrashCan>, IFileTrashCanReadRepository
    {
        public FileTrashCanReadRepository(CraneFileManagerContext context) : base(context)
        {
        }
    }
}
