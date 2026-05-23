using CraneFileManager.Application.Exceptions;
using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Domain.Entities.IdentityAuth;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace CraneFileManager.Application.Validations
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public string[] CustomRoles { get; set; }
        public string[] CustomRolePermissions { get; set; }
        public string[] CustomUserPermissions { get; set; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var user = context.HttpContext.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }


            var userIdClaim = user.Claims.FirstOrDefault(c => c.Value == user.Identity.Name);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User claim not found.");
            }

         

            using (var scope = context.HttpContext.RequestServices.CreateScope())
            {
                var userRoleReadRepository = scope.ServiceProvider.GetRequiredService<IUserRoleReadRepository>();

                var userRoles = userRoleReadRepository.GetAll(false)
                    .Where(ur => ur.User.Username == userIdClaim.Value)
                    .Select(ur => ur.Role.Name)
                    .ToList();

            


                var isAuthorized = CustomRoles.Any(role => userRoles.Contains(role));
                if (!isAuthorized)
                {
                    throw new ForbiddenException($"The role of the user named {user.Identity.Name} is not authorized for this operation.");
                }

                var userPermissionReadRepository = scope.ServiceProvider.GetRequiredService<IUserPermissionReadRepository>();
                var userClaimReadRepository = scope.ServiceProvider.GetRequiredService<IUserClaimReadRepository>();


                var userPermissions = userPermissionReadRepository.GetAll(false)
                    .Where(up => up.Id == userClaimReadRepository.GetAll(false).Where(i => up.Id==i.UserPermitionId).FirstOrDefault().UserPermitionId)
                    .Select(up => up.UserAccess);

                var isAuthorized2 = CustomUserPermissions.Any(userPermission => userPermissions.Contains(userPermission));
                if (!isAuthorized2)
                {
                    throw new ForbiddenException($"The userClaim of the user named {user.Identity.Name} is not authorized for this operation.");
                }


                var rolePermissionReadRepository = scope.ServiceProvider.GetRequiredService<IRolePermissionReadRepository>();
                var roleClaimReadRepository = scope.ServiceProvider.GetRequiredService<IRoleClaimReadRepository>();


                var rolePermissions = rolePermissionReadRepository.GetAll(false)
                    .Where(rp => rp.Id == roleClaimReadRepository.GetAll(false).Where(i => rp.Id == i.RolePermissionId).FirstOrDefault().RolePermissionId)
                    .Select(rp => rp.Method);

                var isAuthorized3 = CustomRolePermissions.Any(rolePermission => rolePermissions.Contains(rolePermission));
                if (!isAuthorized3)
                {
                    throw new ForbiddenException($"The roleClaim of the user named {user.Identity.Name} is not authorized for this operation.");
                }


            }
        }

    }

}
