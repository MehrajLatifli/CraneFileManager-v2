using CraneFileManager.Application.Repositories.Abstract;
using CraneFileManager.Domain.Entities.IdentityAuth;

namespace CraneFileManager.Application.Repositories.Custom
{
    public interface IUserRoleWriteRepository : IWriteRepository<UserRole>
    {
    }
}
