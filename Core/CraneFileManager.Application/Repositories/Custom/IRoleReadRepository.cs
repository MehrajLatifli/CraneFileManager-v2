using CraneFileManager.Application.Repositories.Abstract;
using CraneFileManager.Domain.Entities.IdentityAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Repositories.Custom
{
    public interface IRoleReadRepository : IReadRepository<Role>
    {
    }
}
