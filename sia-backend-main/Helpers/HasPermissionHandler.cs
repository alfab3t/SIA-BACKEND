using astratech_apps_backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace astratech_apps_backend.Helpers
{
    public class HasPermissionHandler(IUserService userService) : AuthorizationHandler<HasPermissionRequirement>
    {
        private readonly IUserService _userService = userService;

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HasPermissionRequirement requirement)
        {
            if (context.Resource is not HttpContext httpContext)
            {
                context.Fail();
                return;
            }

            var endpoint = httpContext.GetEndpoint();
            if (endpoint == null)
            {
                context.Fail();
                return;
            }

            RequiresPermissionAttribute? permissionAttribute = null;

            foreach (var metadataItem in endpoint.Metadata)
            {
                if (metadataItem is RequiresPermissionAttribute attribute)
                {
                    permissionAttribute = attribute;
                    break;
                }
            }

            if (permissionAttribute == null)
            {
                return;
            }

            var requiredPermission = permissionAttribute.Permission;

            var roleId = context.User.FindFirstValue("idrole");
            var appId = context.User.FindFirstValue("idapp");
            var username = context.User.FindFirstValue("namaakun");

            if (string.IsNullOrEmpty(roleId) || string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(username))
            {
                context.Fail();
                return;
            }

            bool hasPermission = await _userService.HasPermissionAsync(username, appId, roleId, requiredPermission);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }
}
