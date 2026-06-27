using Microsoft.AspNetCore.Authorization;

namespace ECommerceAuction.UserService.Api.Authorization;

/// <summary>
/// Protects an endpoint with a fine-grained RBAC privilege policy.
/// </summary>
public sealed class RequirePrivilegeAttribute : AuthorizeAttribute
{
    public const string PrivilegePolicyPrefix = "Privilege:";

    public RequirePrivilegeAttribute(string privilegeCode)
    {
        Policy = $"{PrivilegePolicyPrefix}{privilegeCode}";
    }
}
