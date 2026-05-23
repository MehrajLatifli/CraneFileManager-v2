using CraneFileManager.Application.Repositories.Abstract;

namespace CraneFileManager.Application.Repositories.Custom
{
    public interface IFileTypeReadRepository : IReadRepository<Domain.Entities.Models.FileType>
    {
    }
}
