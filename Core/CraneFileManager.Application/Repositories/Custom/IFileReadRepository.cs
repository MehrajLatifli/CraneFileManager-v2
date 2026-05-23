using CraneFileManager.Application.Repositories.Abstract;

namespace CraneFileManager.Application.Repositories.Custom
{
    public interface IFileReadRepository : IReadRepository<Domain.Entities.Models.File>
    {
    }
}
