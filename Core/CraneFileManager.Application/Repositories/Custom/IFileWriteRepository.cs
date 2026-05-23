using CraneFileManager.Application.Repositories.Abstract;

namespace CraneFileManager.Application.Repositories.Custom
{
    public interface IFileWriteRepository : IWriteRepository<Domain.Entities.Models.File>
    {
    }
}
