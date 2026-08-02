using System.Security.Claims;
using ECommerceAuction.UserService.Api.Grpc;
using ECommerceAuction.UserService.Api.GrpcServices;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using ECommerceAuction.UserService.Domain.Reputation.Policies;
using ECommerceAuction.UserService.Domain.Reputation.Scoring;
using ECommerceAuction.UserService.Domain.Reputation.Seller;
using ECommerceAuction.UserService.Domain.Sellers;
using ECommerceAuction.UserService.Domain.Sellers.Applications;
using ECommerceAuction.UserService.Domain.Users;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace ECommerceAuction.UserService.Application.UnitTests.Reputation.Grpc;

public sealed class ReputationEligibilityGrpcServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 31, 8, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(-1, false)]
    [InlineData(10, false)]
    [InlineData(11, true)]
    public void BuyerAuction_UsesApprovedScoreBoundary(
        int score,
        bool expectedEligible)
    {
        var profile = CreateBuyer(score);

        var result = AuctionEligibilityPolicy.EvaluateBuyer(
            profile,
            isActive: true,
            hasPriorPaymentDefault: false,
            Now);

        Assert.Equal(expectedEligible, result.IsEligible);
    }

    [Fact]
    public void BuyerAuction_DeniesActiveRestriction()
    {
        var profile = CreateBuyer(11);
        profile.ApplyAuctionRestriction(
            ReputationRestrictions.Restricted,
            Now.AddHours(1),
            false,
            null,
            Now);

        var result = AuctionEligibilityPolicy.EvaluateBuyer(
            profile, true, false, Now);

        Assert.False(result.IsEligible);
        Assert.Equal(
            ReputationEligibilityReasonCodes.AuctionRestricted,
            result.ReasonCode);
    }

    [Fact]
    public void BuyerAuction_BlockedRestrictionRequiresExplicitClearAfterDeadline()
    {
        var profile = CreateBuyer(100);
        profile.ApplyAuctionRestriction(
            ReputationRestrictions.Blocked,
            Now.AddMinutes(-1),
            false,
            null,
            Now.AddHours(-1));

        var result = AuctionEligibilityPolicy.EvaluateBuyer(
            profile, true, false, Now);

        Assert.False(result.IsEligible);
        Assert.Equal(
            ReputationEligibilityReasonCodes.AuctionRestricted,
            result.ReasonCode);
    }

    [Fact]
    public void BuyerAuction_DeniesBlockingViolation()
    {
        var profile = CreateBuyer(100);
        profile.ApplyAuctionRestriction(
            ReputationRestrictions.Blocked,
            null,
            false,
            "PAYMENT_DEFAULT",
            Now);

        var result = AuctionEligibilityPolicy.EvaluateBuyer(
            profile, true, false, Now);

        Assert.False(result.IsEligible);
        Assert.Equal(
            ReputationEligibilityReasonCodes.BlockingViolation,
            result.ReasonCode);
    }

    [Fact]
    public void BuyerAuction_DeniesManualReview()
    {
        var profile = CreateBuyer(100);
        profile.ApplyAuctionRestriction(
            ReputationRestrictions.Restricted,
            Now.AddHours(-1),
            true,
            null,
            Now);

        var result = AuctionEligibilityPolicy.EvaluateBuyer(
            profile, true, false, Now);

        Assert.False(result.IsEligible);
        Assert.Equal(
            ReputationEligibilityReasonCodes.ManualReviewRequired,
            result.ReasonCode);
    }

    [Theory]
    [InlineData(11, false)]
    [InlineData(12, true)]
    public void BuyerAuction_PriorDefaultRequiresScoreAboveElevenAndClearedRestriction(
        int score,
        bool expectedEligible)
    {
        var profile = CreateBuyer(score);

        var result = AuctionEligibilityPolicy.EvaluateBuyer(
            profile,
            isActive: true,
            hasPriorPaymentDefault: true,
            Now);

        Assert.Equal(expectedEligible, result.IsEligible);
    }

    [Fact]
    public void BuyerAuction_PriorDefaultBecomesEligibleOnlyAfterAdminClearAndRecovery()
    {
        var profile = CreateBuyer(11);
        profile.ApplyAuctionRestriction(
            ReputationRestrictions.Blocked,
            Now.AddMinutes(-1),
            true,
            "PAYMENT_DEFAULT",
            Now.AddDays(-1));

        Assert.False(
            AuctionEligibilityPolicy.EvaluateBuyer(
                profile, true, true, Now).IsEligible);

        profile.ClearAuctionRestriction(Now);
        Assert.False(
            AuctionEligibilityPolicy.EvaluateBuyer(
                profile, true, true, Now).IsEligible);

        profile.ApplyConfirmedDelta(1, Now);
        Assert.True(
            AuctionEligibilityPolicy.EvaluateBuyer(
                profile, true, true, Now).IsEligible);
    }

    [Theory]
    [InlineData(19, false)]
    [InlineData(20, true)]
    public void SellerAuction_UsesApprovedScoreBoundary(
        int score,
        bool expectedEligible)
    {
        var profile = CreateSeller(score);

        var result = AuctionEligibilityPolicy.EvaluateSeller(
            profile,
            isActive: true,
            Now);

        Assert.Equal(expectedEligible, result.IsEligible);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SellerAuction_DeniesSellingOrAuctionRestriction(
        bool sellingRestriction)
    {
        var profile = CreateSeller(20);
        if (sellingRestriction)
        {
            profile.ApplySellingRestriction(
                ReputationRestrictions.Restricted,
                Now.AddHours(1),
                false,
                null,
                Now);
        }
        else
        {
            profile.ApplyAuctionRestriction(
                ReputationRestrictions.Restricted,
                Now.AddHours(1),
                false,
                null,
                Now);
        }

        var result = AuctionEligibilityPolicy.EvaluateSeller(
            profile, true, Now);

        Assert.False(result.IsEligible);
    }

    [Fact]
    public void SellerAuction_DeniesBlockingViolation()
    {
        var profile = CreateSeller(20);
        profile.ApplyAuctionRestriction(
            ReputationRestrictions.Blocked,
            null,
            false,
            "SHILL_BIDDING",
            Now);

        var result = AuctionEligibilityPolicy.EvaluateSeller(
            profile, true, Now);

        Assert.False(result.IsEligible);
        Assert.Equal(
            ReputationEligibilityReasonCodes.BlockingViolation,
            result.ReasonCode);
    }

    [Fact]
    public void CodEligibility_FailsClosedUntilRuleIsApproved()
    {
        var result = CodEligibilityPolicy.Evaluate();

        Assert.False(result.IsEligible);
        Assert.Equal(
            ReputationEligibilityReasonCodes.CodPolicyNotConfigured,
            result.ReasonCode);
    }

    [Fact]
    public async Task GrpcBuyerAuction_ReturnsEligibilityAndReadsPriorDefaultHistory()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId);
        var profile = CreateBuyer(12, userId);
        var dependencies = CreateGrpcDependencies();
        dependencies.Users.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        dependencies.Buyers.GetByUserIdAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(profile);
        dependencies.Ledger.HasReasonAsync(
                userId,
                ReputationRoles.Buyer,
                ReputationReasonCatalog.BuyerAuctionWinnerPaymentDefault,
                Arg.Any<CancellationToken>())
            .Returns(true);
        var service = dependencies.CreateService();

        var reply = await service.GetBuyerAuctionEligibility(
            new ReputationEligibilityRequest { UserId = userId.ToString() },
            CreateContext());

        Assert.True(reply.Eligible);
        Assert.Equal(12, reply.ConfirmedScore);
        Assert.Equal(profile.TrustLevel, reply.TrustLevel);
        Assert.Equal(ReputationRestrictions.None, reply.RestrictionStatus);
    }

    [Fact]
    public async Task GrpcSellerAuction_ReturnsEligibilityForActiveSeller()
    {
        var userId = Guid.NewGuid();
        var dependencies = CreateGrpcDependencies();
        dependencies.Users.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(CreateUser(userId));
        dependencies.SellerApplications.GetByUserIdAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(CreateApprovedSeller(userId));
        dependencies.Sellers.GetByUserIdAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(CreateSeller(20, userId));
        var service = dependencies.CreateService();

        var reply = await service.GetSellerAuctionEligibility(
            new ReputationEligibilityRequest { UserId = userId.ToString() },
            CreateContext());

        Assert.True(reply.Eligible);
        Assert.Equal(20, reply.ConfirmedScore);
    }

    [Fact]
    public async Task GrpcCodEligibility_FailsClosedWithStructuredReason()
    {
        var userId = Guid.NewGuid();
        var dependencies = CreateGrpcDependencies();
        dependencies.Users.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(CreateUser(userId));
        dependencies.Buyers.GetByUserIdAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(CreateBuyer(100, userId));
        var service = dependencies.CreateService();

        var reply = await service.GetBuyerCodEligibility(
            new ReputationEligibilityRequest { UserId = userId.ToString() },
            CreateContext());

        Assert.False(reply.Eligible);
        Assert.Equal(
            ReputationEligibilityReasonCodes.CodPolicyNotConfigured,
            reply.ReasonCode);
    }

    [Fact]
    public async Task GrpcService_RejectsInvalidUserId()
    {
        var service = CreateGrpcDependencies().CreateService();

        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            service.GetBuyerAuctionEligibility(
                new ReputationEligibilityRequest { UserId = "invalid" },
                CreateContext()));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task GrpcService_ReturnsNotFoundForMissingProfile()
    {
        var userId = Guid.NewGuid();
        var dependencies = CreateGrpcDependencies();
        dependencies.Users.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(CreateUser(userId));
        var service = dependencies.CreateService();

        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            service.GetBuyerAuctionEligibility(
                new ReputationEligibilityRequest
                {
                    UserId = userId.ToString()
                },
                CreateContext()));

        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GrpcService_RejectsCallerWithoutInternalScope()
    {
        var service = CreateGrpcDependencies().CreateService();

        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            service.GetBuyerAuctionEligibility(
                new ReputationEligibilityRequest
                {
                    UserId = Guid.NewGuid().ToString()
                },
                CreateContext(authorized: false)));

        Assert.Equal(StatusCode.PermissionDenied, exception.StatusCode);
    }

    [Fact]
    public async Task GrpcService_PropagatesCancellationToken()
    {
        var userId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var dependencies = CreateGrpcDependencies();
        dependencies.Users.GetByIdAsync(userId, cancellation.Token)
            .Returns<Task<User?>>(_ =>
                Task.FromCanceled<User?>(cancellation.Token));
        var service = dependencies.CreateService();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.GetBuyerAuctionEligibility(
                new ReputationEligibilityRequest
                {
                    UserId = userId.ToString()
                },
                CreateContext(cancellationToken: cancellation.Token)));
    }

    private static BuyerReputationProfile CreateBuyer(int score)
        => CreateBuyer(score, Guid.NewGuid());

    private static BuyerReputationProfile CreateBuyer(int score, Guid userId)
    {
        var profile = BuyerReputationProfile.Create(userId, Now);
        if (score != 0)
            profile.ApplyConfirmedDelta(score, Now);
        return profile;
    }

    private static SellerReputationProfile CreateSeller(int score)
        => CreateSeller(score, Guid.NewGuid());

    private static SellerReputationProfile CreateSeller(int score, Guid userId)
    {
        var profile = SellerReputationProfile.Create(userId, Now);
        if (score != 0)
            profile.ApplyConfirmedDelta(score, Now);
        return profile;
    }

    private static User CreateUser(Guid userId) =>
        new(
            userId,
            $"user-{userId:N}@example.com",
            "hash",
            "Test User",
            "0900000000");

    private static SellerProfile CreateApprovedSeller(Guid userId)
    {
        var profile = SellerProfile.Create(
            userId,
            "Individual",
            "Test Shop",
            "0900000000",
            "TAX123",
            "license-url",
            "address",
            "123456789",
            "Test Bank",
            "TEST USER",
            Now);
        profile.Approve(Guid.NewGuid(), Now);
        return profile;
    }

    private static GrpcDependencies CreateGrpcDependencies() =>
        new(
            Substitute.For<IUserRepository>(),
            Substitute.For<IBuyerReputationRepository>(),
            Substitute.For<ISellerReputationRepository>(),
            Substitute.For<ISellerProfileRepository>(),
            Substitute.For<IReputationLedgerRepository>());

    private static ServerCallContext CreateContext(
        bool authorized = true,
        CancellationToken cancellationToken = default)
    {
        var claims = authorized
            ? new[]
            {
                new Claim("token_use", "service"),
                new Claim(
                    "scope",
                    InternalScopes.CommerceEligibilityRead)
            }
            : new[] { new Claim("token_use", "service") };
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, "test"));
        return new TestServerCallContext(principal, cancellationToken);
    }

    private sealed record GrpcDependencies(
        IUserRepository Users,
        IBuyerReputationRepository Buyers,
        ISellerReputationRepository Sellers,
        ISellerProfileRepository SellerApplications,
        IReputationLedgerRepository Ledger)
    {
        public ReputationEligibilityGrpcService CreateService() =>
            new(
                Users,
                Buyers,
                Sellers,
                SellerApplications,
                Ledger,
                new FixedTimeProvider(Now));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class TestServerCallContext : ServerCallContext
    {
        private readonly Metadata _requestHeaders = [];
        private readonly Metadata _responseTrailers = [];
        private readonly CancellationToken _cancellationToken;
        private readonly Dictionary<object, object> _userState = [];
        private Status _status;
        private WriteOptions? _writeOptions;

        public TestServerCallContext(
            ClaimsPrincipal principal,
            CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            var httpContext = new DefaultHttpContext
            {
                User = principal
            };
            _userState["__HttpContext"] = httpContext;
        }

        protected override string MethodCore => "reputation-eligibility";
        protected override string HostCore => "localhost";
        protected override string PeerCore => "test";
        protected override DateTime DeadlineCore => DateTime.MaxValue;
        protected override Metadata RequestHeadersCore => _requestHeaders;
        protected override CancellationToken CancellationTokenCore =>
            _cancellationToken;
        protected override Metadata ResponseTrailersCore => _responseTrailers;
        protected override Status StatusCore
        {
            get => _status;
            set => _status = value;
        }
        protected override WriteOptions? WriteOptionsCore
        {
            get => _writeOptions;
            set => _writeOptions = value;
        }
        protected override AuthContext AuthContextCore =>
            new(string.Empty, []);
        protected override ContextPropagationToken CreatePropagationTokenCore(
            ContextPropagationOptions? options) =>
            throw new NotSupportedException();
        protected override Task WriteResponseHeadersAsyncCore(
            Metadata responseHeaders) =>
            Task.CompletedTask;
        protected override IDictionary<object, object> UserStateCore =>
            _userState;
    }
}
