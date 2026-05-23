using CraneFileManager.Application.Repositories.Abstract;

namespace CraneFileManager.Application.Repositories.Custom
{
    public interface IFileTypeWriteRepository : IWriteRepository<Domain.Entities.Models.FileType>
    {
    }
}
