using Microsoft.AspNetCore.Authorization;

namespace ECommerceAuction.UserService.Api.Authorization;

public sealed record PrivilegeRequirement(string PrivilegeCode) : IAuthorizationRequirement;

/// <summary>
/// Succeeds only when the authenticated JWT contains the requested privilege claim.
/// </summary>
public sealed class PrivilegeAuthorizationHandler : AuthorizationHandler<PrivilegeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PrivilegeRequirement requirement)
    {
        var hasPrivilege = context.User.Claims.Any(claim =>
            claim.Type == "privilege" &&
            string.Equals(
                claim.Value,
                requirement.PrivilegeCode,
                StringComparison.OrdinalIgnoreCase));

        if (hasPrivilege)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
