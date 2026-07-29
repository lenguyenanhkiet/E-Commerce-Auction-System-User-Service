using Grpc.Core;
using Microsoft.IdentityModel.Tokens;
using Nexus.Contracts.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerceAuction.UserService.Infrastructure.Authentication;

namespace ECommerceAuction.UserService.Api.GrpcServices;

/// <summary>
/// Scope names granted to internal service tokens for the User Service gRPC contracts.
/// </summary>
public static class InternalScopes
{
    public const string SellerEligibilityRead = "user.internal.seller-eligibility.read";
    public const string UserProfileRead = "user.internal.user-profile.read";
    public const string CommerceEligibilityRead = "user.internal.commerce.eligibility.read";
    public const string CommerceSellerProfileRead = "user.internal.commerce.seller-profile.read";
}

/// <summary>
/// Enforces that the caller presented a service token (token_use=service) carrying the
/// required scope. The [Authorize(AuthenticationSchemes = ServiceJwt)] attribute already
/// guarantees a valid service token; this adds the per-RPC scope check.
/// </summary>
internal static class InternalServiceCallGuard
{
    public static void RequireCommerceCaller(ServerCallContext context, string requiredScope, JwtOptions jwtOptions)
    {
        var user = ValidateServiceToken(context, jwtOptions);
        RequireMetadata(context);
        RequireScope(user, requiredScope, "commerce-service", "user-service");
    }

    public static void RequireScope(ServerCallContext context, string requiredScope)
    {
        RequireScope(context, requiredScope, expectedClientId: null, expectedAudience: null);
    }

    private static void RequireMetadata(ServerCallContext context)
    {
        var correlationId = context.RequestHeaders.GetValue(GrpcMetadataNames.CorrelationId);
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw Error(
                StatusCode.InvalidArgument,
                GrpcCommonErrorCodes.CorrelationIdRequired,
                "CORRELATION_ID_REQUIRED");
        }
    }

    private static void RequireScope(
        ServerCallContext context,
        string requiredScope,
        string? expectedClientId,
        string? expectedAudience)
    {
        RequireScope(context.GetHttpContext().User, requiredScope, expectedClientId, expectedAudience);
    }

    private static void RequireScope(
        ClaimsPrincipal user,
        string requiredScope,
        string? expectedClientId,
        string? expectedAudience)
    {
        var isService = string.Equals(
            user.FindFirst("token_use")?.Value,
            "service",
            StringComparison.OrdinalIgnoreCase);
        var clientMatches = string.IsNullOrWhiteSpace(expectedClientId) ||
                            string.Equals(
                                user.FindFirst("client_id")?.Value,
                                expectedClientId,
                                StringComparison.Ordinal);
        var audienceMatches = string.IsNullOrWhiteSpace(expectedAudience) ||
                              user.FindAll("aud").Any(claim =>
                                  string.Equals(claim.Value, expectedAudience, StringComparison.Ordinal));

        var hasScope = user.FindAll("scope")
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Any(scope => string.Equals(scope, requiredScope, StringComparison.Ordinal));

        if (!isService || !clientMatches || !audienceMatches || !hasScope)
        {
            throw Error(
                StatusCode.PermissionDenied,
                GrpcCommonErrorCodes.ServiceScopeForbidden,
                "INSUFFICIENT_SCOPE");
        }
    }

    private static ClaimsPrincipal ValidateServiceToken(ServerCallContext context, JwtOptions jwtOptions)
    {
        var authorization = context.RequestHeaders.GetValue(GrpcMetadataNames.Authorization);
        if (string.IsNullOrWhiteSpace(authorization) ||
            !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            throw Error(
                StatusCode.Unauthenticated,
                GrpcCommonErrorCodes.ServiceTokenInvalid,
                GrpcCommonErrorCodes.ServiceTokenInvalid);
        }

        var token = authorization["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw Error(
                StatusCode.Unauthenticated,
                GrpcCommonErrorCodes.ServiceTokenInvalid,
                GrpcCommonErrorCodes.ServiceTokenInvalid);
        }

        try
        {
            return new JwtSecurityTokenHandler().ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                },
                out _);
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            throw Error(
                StatusCode.Unauthenticated,
                GrpcCommonErrorCodes.ServiceTokenInvalid,
                GrpcCommonErrorCodes.ServiceTokenInvalid);
        }
    }

    public static RpcException Error(StatusCode statusCode, string errorCode, string detail) =>
        new(
            new Status(statusCode, detail),
            new Metadata { { GrpcMetadataNames.ErrorCode, errorCode } });
}
