using Microsoft.AspNetCore.Authorization;

namespace ECommerceAuction.UserService.Api.Authorization;

/// <summary>
/// Check if the token contains the permission required by <see cref="PermissionRequirement"/>.
/// The token carries multiple individual "privilege" claims (one permission per claim) — NOT a combined array.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    /// <summary>
    /// The claim name contains permission. It must match the user-service name recorded when signing the token.
    /// </summary>
    public const string PermissionClaimType = "privilege";
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var hasPermission = context.User
            .FindAll(PermissionClaimType)
            .Any(claim => string.Equals(
                claim.Value,
                requirement.Permission,
                StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        // Do not call context.Fail(): so that other handlers/policies (if any) still have a chance to evaluate.
        return Task.CompletedTask;
    }
}
