using Grpc.Core;

namespace ECommerceAuction.UserService.Api.GrpcServices;

/// <summary>
/// Scope names granted to internal service tokens for the User Service gRPC contracts.
/// </summary>
public static class InternalScopes
{
    public const string SellerEligibilityRead = "user.internal.seller-eligibility.read";
    public const string UserProfileRead = "user.internal.user-profile.read";
}

/// <summary>
/// Enforces that the caller presented a service token (token_use=service) carrying the
/// required scope. The [Authorize(AuthenticationSchemes = ServiceJwt)] attribute already
/// guarantees a valid service token; this adds the per-RPC scope check.
/// </summary>
internal static class InternalServiceCallGuard
{
    public static void RequireScope(ServerCallContext context, string requiredScope)
    {
        var user = context.GetHttpContext().User;

        var isService = string.Equals(
            user.FindFirst("token_use")?.Value,
            "service",
            StringComparison.OrdinalIgnoreCase);

        var hasScope = user.FindAll("scope")
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Any(scope => string.Equals(scope, requiredScope, StringComparison.Ordinal));

        if (!isService || !hasScope)
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, "INSUFFICIENT_SCOPE"));
        }
    }
}
