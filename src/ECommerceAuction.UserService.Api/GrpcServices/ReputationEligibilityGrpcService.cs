using ECommerceAuction.UserService.Api.Grpc;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using ECommerceAuction.UserService.Domain.Reputation.Policies;
using ECommerceAuction.UserService.Domain.Reputation.Scoring;
using ECommerceAuction.UserService.Domain.Reputation.Seller;
using ECommerceAuction.UserService.Domain.Sellers;
using ECommerceAuction.UserService.Domain.Sellers.Applications;
using ECommerceAuction.UserService.Domain.Users;
using ECommerceAuction.UserService.Infrastructure.Authentication;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace ECommerceAuction.UserService.Api.GrpcServices;

[Authorize(AuthenticationSchemes = ServiceAuthSchemes.ServiceJwt)]
public sealed class ReputationEligibilityGrpcService
    : ReputationEligibility.ReputationEligibilityBase
{
    private readonly IUserRepository _users;
    private readonly IBuyerReputationRepository _buyers;
    private readonly ISellerReputationRepository _sellers;
    private readonly ISellerProfileRepository _sellerApplications;
    private readonly IReputationLedgerRepository _ledger;
    private readonly TimeProvider _timeProvider;

    public ReputationEligibilityGrpcService(
        IUserRepository users,
        IBuyerReputationRepository buyers,
        ISellerReputationRepository sellers,
        ISellerProfileRepository sellerApplications,
        IReputationLedgerRepository ledger,
        TimeProvider timeProvider)
    {
        _users = users;
        _buyers = buyers;
        _sellers = sellers;
        _sellerApplications = sellerApplications;
        _ledger = ledger;
        _timeProvider = timeProvider;
    }

    public override async Task<ReputationEligibilityReply>
        GetBuyerAuctionEligibility(
            ReputationEligibilityRequest request,
            ServerCallContext context)
    {
        RequireInternalCaller(context);
        var userId = ParseUserId(request.UserId);
        var user = await GetUserAsync(userId, context.CancellationToken);
        var profile = await _buyers.GetByUserIdAsync(
            userId,
            context.CancellationToken);
        if (profile is null)
            throw NotFound("BUYER_REPUTATION_PROFILE_NOT_FOUND");

        var hasPriorDefault = await _ledger.HasReasonAsync(
            userId,
            ReputationRoles.Buyer,
            ReputationReasonCatalog.BuyerAuctionWinnerPaymentDefault,
            context.CancellationToken);
        var result = AuctionEligibilityPolicy.EvaluateBuyer(
            profile,
            IsActive(user),
            hasPriorDefault,
            _timeProvider.GetUtcNow());

        return Map(
            result,
            profile.ConfirmedScore,
            profile.TrustLevel,
            profile.AuctionRestrictionStatus,
            profile.RequiresManualReview,
            profile.RestrictedUntil);
    }

    public override async Task<ReputationEligibilityReply>
        GetSellerAuctionEligibility(
            ReputationEligibilityRequest request,
            ServerCallContext context)
    {
        RequireInternalCaller(context);
        var userId = ParseUserId(request.UserId);
        var user = await GetUserAsync(userId, context.CancellationToken);
        var sellerApplication =
            await _sellerApplications.GetByUserIdAsync(
                userId,
                context.CancellationToken);
        if (sellerApplication is null)
            throw NotFound("SELLER_PROFILE_NOT_FOUND");

        var profile = await _sellers.GetByUserIdAsync(
            userId,
            context.CancellationToken);
        if (profile is null)
            throw NotFound("SELLER_REPUTATION_PROFILE_NOT_FOUND");

        var sellerActive =
            IsActive(user) &&
            sellerApplication.DeletedAt is null &&
            sellerApplication.Status == SellerApplicationStatus.Approved;
        var result = AuctionEligibilityPolicy.EvaluateSeller(
            profile,
            sellerActive,
            _timeProvider.GetUtcNow());
        var restrictionStatus =
            profile.SellingRestrictionStatus != ReputationRestrictions.None
                ? profile.SellingRestrictionStatus
                : profile.AuctionRestrictionStatus;

        return Map(
            result,
            profile.ConfirmedScore,
            profile.TrustLevel,
            restrictionStatus,
            profile.RequiresManualReview,
            profile.RestrictedUntil);
    }

    public override async Task<ReputationEligibilityReply>
        GetBuyerCodEligibility(
            ReputationEligibilityRequest request,
            ServerCallContext context)
    {
        RequireInternalCaller(context);
        var userId = ParseUserId(request.UserId);
        await GetUserAsync(userId, context.CancellationToken);
        var profile = await _buyers.GetByUserIdAsync(
            userId,
            context.CancellationToken);
        if (profile is null)
            throw NotFound("BUYER_REPUTATION_PROFILE_NOT_FOUND");

        return Map(
            CodEligibilityPolicy.Evaluate(),
            profile.ConfirmedScore,
            profile.TrustLevel,
            profile.AuctionRestrictionStatus,
            profile.RequiresManualReview,
            profile.RestrictedUntil);
    }

    private static void RequireInternalCaller(ServerCallContext context) =>
        InternalServiceCallGuard.RequireScope(
            context,
            InternalScopes.CommerceEligibilityRead);

    private async Task<User> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        return user ?? throw NotFound("USER_NOT_FOUND");
    }

    private static Guid ParseUserId(string value)
    {
        if (!Guid.TryParse(value, out var userId) || userId == Guid.Empty)
        {
            throw InternalServiceCallGuard.Error(
                StatusCode.InvalidArgument,
                "USER_ID_INVALID",
                "USER_ID_INVALID");
        }

        return userId;
    }

    private static bool IsActive(User user) =>
        user.DeletedAt is null && user.Status == UserStatus.Active;

    private static RpcException NotFound(string code) =>
        InternalServiceCallGuard.Error(
            StatusCode.NotFound,
            code,
            code);

    private static ReputationEligibilityReply Map(
        ReputationEligibilityResult result,
        int confirmedScore,
        string trustLevel,
        string restrictionStatus,
        bool requiresManualReview,
        DateTimeOffset? restrictedUntil)
    {
        var reply = new ReputationEligibilityReply
        {
            Eligible = result.IsEligible,
            ReasonCode = result.ReasonCode,
            ConfirmedScore = confirmedScore,
            TrustLevel = trustLevel,
            RestrictionStatus = restrictionStatus,
            RequiresManualReview = requiresManualReview
        };

        if (restrictedUntil.HasValue)
        {
            reply.RestrictedUntil = Timestamp.FromDateTime(
                restrictedUntil.Value.UtcDateTime);
        }

        return reply;
    }
}
