using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Domain.Entities.Models;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using CraneFileManager.Persistence.Repositories.Concrete;

namespace CraneFileManager.Persistence.Repositories.Custom
{
    public class NotificationReadRepository : ReadRepository<Notification>, INotificationReadRepository
    {
        public NotificationReadRepository(CraneFileManagerContext context) : base(context)
        {
        }
    }

}
