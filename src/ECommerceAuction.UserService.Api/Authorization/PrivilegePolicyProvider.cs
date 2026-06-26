using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ECommerceAuction.UserService.Api.Authorization;

/// <summary>
/// Builds privilege policies on demand from names such as Privilege:ROLE.CREATE.
/// </summary>
public sealed class PrivilegePolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PrivilegePolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(
                RequirePrivilegeAttribute.PrivilegePolicyPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return base.GetPolicyAsync(policyName);
        }

        var privilegeCode = policyName[
            RequirePrivilegeAttribute.PrivilegePolicyPrefix.Length..];

        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PrivilegeRequirement(privilegeCode))
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}
