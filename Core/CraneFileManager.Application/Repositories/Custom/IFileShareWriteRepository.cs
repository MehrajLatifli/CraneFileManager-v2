using CraneFileManager.Application.Repositories.Abstract;

namespace CraneFileManager.Application.Repositories.Custom
{
    public interface IFileShareWriteRepository : IWriteRepository<Domain.Entities.Models.FileShare>
    {
    }
}
