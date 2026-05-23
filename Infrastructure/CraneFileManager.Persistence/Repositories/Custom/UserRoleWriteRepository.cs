using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Domain.Entities.IdentityAuth;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using CraneFileManager.Persistence.Repositories.Concrete;

namespace CraneFileManager.Persistence.Repositories.Custom
{
    public class UserRoleWriteRepository : WriteRepository<UserRole>, IUserRoleWriteRepository
    {
        public UserRoleWriteRepository(CraneFileManagerContext context) : base(context)
        {

        }
    }


}
