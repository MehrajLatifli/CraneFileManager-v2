using CraneFileManager.Application.Repositories.Abstract;

namespace CraneFileManager.Application.Repositories.Custom
{
    public interface IUserFileWriteRepository : IWriteRepository<Domain.Entities.Models.UserFile>
    {
    }
}
