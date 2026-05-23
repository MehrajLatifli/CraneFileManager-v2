using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Domain.Entities.IdentityAuth;
using CraneFileManager.Domain.Entities.Models;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using CraneFileManager.Persistence.Repositories.Concrete;

namespace CraneFileManager.Persistence.Repositories.Custom
{
    public class UserNotificationWriteRepository : WriteRepository<UserNotification>, IUserNotificationWriteRepository
    {
        public UserNotificationWriteRepository(CraneFileManagerContext context) : base(context)
        {
        }
    }

}
