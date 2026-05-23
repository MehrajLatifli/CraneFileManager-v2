using CraneFileManager.Application.Repositories.Abstract;
using CraneFileManager.Domain.Entities.Models;

namespace CraneFileManager.Application.Repositories.Custom
{
    public interface INotificationWriteRepository : IWriteRepository<Notification>
    {
    }
}
