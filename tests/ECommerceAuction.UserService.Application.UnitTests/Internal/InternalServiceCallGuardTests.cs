using System.Security.Claims;
using ECommerceAuction.UserService.Api.GrpcServices;
using Grpc.Core;
using Microsoft.AspNetCore.Http;

namespace ECommerceAuction.UserService.Application.UnitTests.Internal;

public sealed class InternalServiceCallGuardTests
{
    [Fact]
    public void Commerce_eligibility_scope_from_service_token_is_accepted()
    {
        var context = CreateContext("service", InternalScopes.CommerceEligibilityRead);

        InternalServiceCallGuard.RequireScope(context, InternalScopes.CommerceEligibilityRead);
    }

    [Fact]
    public void Missing_required_scope_is_permission_denied()
    {
        var context = CreateContext("service", InternalScopes.CommerceSellerProfileRead);

        var exception = Assert.Throws<RpcException>(() =>
            InternalServiceCallGuard.RequireScope(context, InternalScopes.CommerceEligibilityRead));

        Assert.Equal(StatusCode.PermissionDenied, exception.StatusCode);
        Assert.Equal("INSUFFICIENT_SCOPE", exception.Status.Detail);
    }

    [Fact]
    public void Normal_user_bearer_claims_are_permission_denied_even_with_scope_text()
    {
        var context = CreateContext("access", InternalScopes.CommerceEligibilityRead);

        var exception = Assert.Throws<RpcException>(() =>
            InternalServiceCallGuard.RequireScope(context, InternalScopes.CommerceEligibilityRead));

        Assert.Equal(StatusCode.PermissionDenied, exception.StatusCode);
    }

    [Theory]
    [InlineData("commerce-service", InternalScopes.CommerceEligibilityRead)]
    [InlineData("commerce-service", InternalScopes.CommerceSellerProfileRead)]
    [InlineData("catalog-service", InternalScopes.SellerEligibilityRead)]
    public void Existing_internal_service_scopes_are_independently_accepted(
        string clientId,
        string scope)
    {
        var context = CreateContext("service", scope, clientId);

        InternalServiceCallGuard.RequireScope(context, scope);
    }

    private static ServerCallContext CreateContext(
        string tokenUse,
        string scope,
        string clientId = "commerce-service")
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("token_use", tokenUse),
                new Claim("client_id", clientId),
                new Claim("scope", scope)
            ], "test-service-token"))
        };

        return new TestServerCallContext(httpContext);
    }

    private sealed class TestServerCallContext(HttpContext httpContext) : ServerCallContext
    {
        private readonly IDictionary<object, object> _userState =
            new Dictionary<object, object> { ["__HttpContext"] = httpContext };

        protected override string MethodCore => "test.UserCommerceGrpc/GetBuyerCheckoutEligibility";
        protected override string HostCore => "localhost";
        protected override string PeerCore => "ipv4:127.0.0.1:0";
        protected override DateTime DeadlineCore => DateTime.UtcNow.AddMinutes(1);
        protected override Metadata RequestHeadersCore { get; } = [];
        protected override CancellationToken CancellationTokenCore => CancellationToken.None;
        protected override Metadata ResponseTrailersCore { get; } = [];
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore { get; } =
            new("test", new Dictionary<string, List<AuthProperty>>());
        protected override IDictionary<object, object> UserStateCore => _userState;
        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options) =>
            throw new InvalidOperationException("Propagation is not used by these guard tests.");
        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => Task.CompletedTask;
    }
}
