using CraneFileManager.Application.Repositories.Abstract;

namespace CraneFileManager.Application.Repositories.Custom
{
    public interface IFileTrashCanWriteRepository : IWriteRepository<Domain.Entities.Models.FileTrashCan>
    {
    }
}
