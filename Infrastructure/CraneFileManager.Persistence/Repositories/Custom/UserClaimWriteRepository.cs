using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Domain.Entities.IdentityAuth;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using CraneFileManager.Persistence.Repositories.Concrete;

namespace CraneFileManager.Persistence.Repositories.Custom
{
    public class UserClaimWriteRepository : WriteRepository<UserClaim>, IUserClaimWriteRepository
    {
        public UserClaimWriteRepository(CraneFileManagerContext context) : base(context)
        {

        }
    }


}
