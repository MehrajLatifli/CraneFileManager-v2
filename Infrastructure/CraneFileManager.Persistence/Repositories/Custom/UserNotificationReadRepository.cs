using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Domain.Entities.IdentityAuth;
using CraneFileManager.Domain.Entities.Models;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using CraneFileManager.Persistence.Repositories.Concrete;

namespace CraneFileManager.Persistence.Repositories.Custom
{
    public class UserNotificationReadRepository : ReadRepository<UserNotification>, IUserNotificationReadRepository
    {
        public UserNotificationReadRepository(CraneFileManagerContext context) : base(context)
        {
        }
    }

}
