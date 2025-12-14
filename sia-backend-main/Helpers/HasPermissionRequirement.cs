using Microsoft.AspNetCore.Authorization;

namespace astratech_apps_backend.Helpers
{
    public class HasPermissionRequirement(string permission) : IAuthorizationRequirement
    {
        public string Permission { get; } = permission;
    }
}
