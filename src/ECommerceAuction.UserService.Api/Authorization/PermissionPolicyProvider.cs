using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ECommerceAuction.UserService.Api.Authorization;
/// <summary>
/// Generates a dynamic authorization policy at runtime. When the controller declares [Authorize(Policy = "USER.CREATE")]
/// and no policy with that name is already registered, this provider automatically creates a policy containing
/// PermissionRequirement("USER.CREATE"). This eliminates the need to manually add a policy for each permission.
/// Mechanism: Every policy name is considered a permission code. If you want to differentiate later
/// (for example, only uppercase names with a dot are considered permissions), add a condition in GetPolicyAsync.
/// </summary>
public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    // Default Provider of ASP.NET Core: used for GetDefaultPolicy / GetFallbackPolicy and for unnamed [Authorize] policies (as long as they are authenticated).
    private readonly DefaultAuthorizationPolicyProvider _fallbackProvider;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackProvider = new DefaultAuthorizationPolicyProvider(options);
    }
    /// <summary>
    /// Default Policy (applies to unnamed [Authorize]): Authentication required.
    /// </summary>
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackProvider.GetDefaultPolicyAsync();
    /// <summary>
    /// Fallback policy: keep default (null = do not force global policy).
    /// </summary>
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackProvider.GetFallbackPolicyAsync();
    /// <summary>
    // Called for each policy name above [Authorize(Policy = "...")].
    // Returns the policy containing the corresponding PermissionRequirement.
    /// </summary>
    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName)).Build();
        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}
