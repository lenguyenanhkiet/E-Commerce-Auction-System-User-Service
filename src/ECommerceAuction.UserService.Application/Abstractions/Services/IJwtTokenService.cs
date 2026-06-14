namespace ECommerceAuction.UserService.Application.Abstractions.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles);
}

