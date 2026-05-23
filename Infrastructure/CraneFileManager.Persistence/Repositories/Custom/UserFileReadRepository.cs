using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using CraneFileManager.Persistence.Repositories.Concrete;

namespace CraneFileManager.Persistence.Repositories.Custom
{
    public class UserFileReadRepository : ReadRepository<Domain.Entities.Models.UserFile>, IUserFileReadRepository
    {
        public UserFileReadRepository(CraneFileManagerContext context) : base(context)
        {
        }
    }
}
