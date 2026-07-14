using Microsoft.AspNetCore.Authorization;

namespace ECommerceAuction.UserService.Api.Authorization;

/// <summary>
/// Authorization required: the request must have a specific permission (e.g., "USER.CREATE)
/// This is plain data — the validation logic is in <see cref="PermissionAuthorizationHandler"/>.
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
    public string Permission { get; }
}
