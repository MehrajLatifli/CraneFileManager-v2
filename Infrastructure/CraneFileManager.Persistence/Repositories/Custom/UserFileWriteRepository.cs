using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using CraneFileManager.Persistence.Repositories.Concrete;

namespace CraneFileManager.Persistence.Repositories.Custom
{
    public class UserFileWriteRepository : WriteRepository<Domain.Entities.Models.UserFile>, IUserFileWriteRepository
    {
        public UserFileWriteRepository(CraneFileManagerContext context) : base(context)
        {
        }
    }
}
