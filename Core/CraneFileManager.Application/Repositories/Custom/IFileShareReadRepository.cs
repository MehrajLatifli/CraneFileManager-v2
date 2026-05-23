using CraneFileManager.Application.Repositories.Abstract;

namespace CraneFileManager.Application.Repositories.Custom
{
    public interface IFileShareReadRepository : IReadRepository<Domain.Entities.Models.FileShare>
    {
    }
}
