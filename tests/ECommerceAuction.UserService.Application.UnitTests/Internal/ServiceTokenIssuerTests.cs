using System.IdentityModel.Tokens.Jwt;
using ECommerceAuction.UserService.Infrastructure.Authentication;
using Microsoft.Extensions.Options;

namespace ECommerceAuction.UserService.Application.UnitTests.Internal;

public sealed class ServiceTokenIssuerTests
{
    [Theory]
    [InlineData("user-service", "user.internal.commerce.eligibility.read")]
    [InlineData("catalog-service", "catalog.internal.commerce.read")]
    [InlineData("wallet-service", "wallet.commerce")]
    public void Issue_AllowsConfiguredCommerceAudiencesAndScopes(
        string audience,
        string scope)
    {
        var issuer = CreateIssuer();

        var result = issuer.Issue("commerce-service", "commerce-secret", audience, scope);

        Assert.NotNull(result);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result!.AccessToken);
        Assert.Contains(audience, token.Audiences);
        Assert.Equal("commerce-service", token.Claims.Single(claim => claim.Type == "client_id").Value);
        Assert.Equal(scope, token.Claims.Single(claim => claim.Type == "scope").Value);
    }

    [Fact]
    public void Issue_RejectsAudienceNotAllowedForClient()
    {
        var issuer = CreateIssuer();

        var result = issuer.Issue(
            "commerce-service",
            "commerce-secret",
            "auction-service",
            "catalog.internal.commerce.read");

        Assert.Null(result);
    }

    [Fact]
    public void Issue_RejectsScopeNotAllowedForAudienceClient()
    {
        var issuer = CreateIssuer();

        var result = issuer.Issue(
            "commerce-service",
            "commerce-secret",
            "wallet-service",
            "wallet.admin");

        Assert.Null(result);
    }

    [Fact]
    public void Issue_PreservesLegacyDefaultAudienceWhenClientHasNoExplicitAudiences()
    {
        var issuer = CreateIssuer(new InternalClient
        {
            ClientId = "legacy-service",
            ClientSecret = "legacy-secret",
            AllowedScopes = ["user.internal.seller-eligibility.read"]
        });

        var result = issuer.Issue(
            "legacy-service",
            "legacy-secret",
            "user-service",
            "user.internal.seller-eligibility.read");

        Assert.NotNull(result);
    }

    private static ServiceTokenIssuer CreateIssuer(params InternalClient[] extraClients)
    {
        var clients = new List<InternalClient>
        {
            new()
            {
                ClientId = "commerce-service",
                ClientSecret = "commerce-secret",
                AllowedAudiences = ["user-service", "catalog-service", "wallet-service"],
                AllowedScopes =
                [
                    "user.internal.commerce.eligibility.read",
                    "user.internal.commerce.seller-profile.read",
                    "catalog.internal.commerce.read",
                    "wallet.commerce"
                ]
            }
        };
        clients.AddRange(extraClients);

        return new ServiceTokenIssuer(
            Options.Create(new JwtOptions
            {
                Issuer = "ECommerceAuction.UserService",
                Audience = "ECommerceAuctionClient",
                InternalAudience = "user-service",
                SecretKey = "TEST_USER_SERVICE_SECRET_KEY_32_CHARS_2026",
                ServiceTokenExpirationMinutes = 5
            }),
            Options.Create(new InternalAuthOptions { Clients = clients }));
    }
}
