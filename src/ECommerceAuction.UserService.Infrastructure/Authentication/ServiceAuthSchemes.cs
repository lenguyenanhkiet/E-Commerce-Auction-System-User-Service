namespace ECommerceAuction.UserService.Infrastructure.Authentication;

/// <summary>
/// Authentication scheme names used by the User Service. The default JwtBearer
/// scheme validates user tokens; <see cref="ServiceJwt"/> validates internal
/// service-to-service tokens (token_use=service, audience = Jwt:InternalAudience).
/// </summary>
public static class ServiceAuthSchemes
{
    public const string ServiceJwt = "ServiceJwt";
}
