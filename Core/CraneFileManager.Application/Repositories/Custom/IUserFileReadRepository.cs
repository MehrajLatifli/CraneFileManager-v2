using CraneFileManager.Application.Repositories.Abstract;

namespace CraneFileManager.Application.Repositories.Custom
{
    public interface IUserFileReadRepository : IReadRepository<Domain.Entities.Models.UserFile>
    {
    }
}
