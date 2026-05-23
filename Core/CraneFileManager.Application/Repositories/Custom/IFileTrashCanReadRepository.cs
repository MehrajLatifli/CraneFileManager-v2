using CraneFileManager.Application.Repositories.Abstract;

namespace CraneFileManager.Application.Repositories.Custom
{
    public interface IFileTrashCanReadRepository : IReadRepository<Domain.Entities.Models.FileTrashCan>
    {
    }
}
