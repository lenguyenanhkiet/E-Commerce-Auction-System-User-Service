using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ECommerceAuction.UserService.Application.Abstractions.Services;

namespace ECommerceAuction.UserService.Infrastructure.Authentication;

public sealed class JwtTokenService : IJwtTokenService
{
    // Duy viết logic tạo JWT 
    public string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles)
    {
        throw new NotImplementedException();
    }
}

