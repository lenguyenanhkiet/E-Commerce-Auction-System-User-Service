using ECommerceAuction.Contracts.User.Grpc;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Infrastructure.Authentication;
using Grpc.Core;
using Microsoft.Extensions.Options;
using Nexus.Shared.Contracts.Common;

namespace ECommerceAuction.UserService.Api.GrpcServices;

/// <summary>
/// Internal Commerce-facing User contract. This adapter is read-only and uses existing
/// User/Seller state without changing login, registration, profile, or seller workflows.
/// </summary>
public sealed class UserCommerceGrpcService : UserCommerceGrpc.UserCommerceGrpcBase
{
    private readonly IInternalUserQueries _queries;
    private readonly JwtOptions _jwtOptions;

    public UserCommerceGrpcService(
        IInternalUserQueries queries,
        IOptions<JwtOptions> jwtOptions)
    {
        _queries = queries;
        _jwtOptions = jwtOptions.Value;
    }

    public override async Task<BuyerCheckoutEligibilityGrpcReply> GetBuyerCheckoutEligibility(
        BuyerCheckoutEligibilityGrpcRequest request,
        ServerCallContext context)
    {
        InternalServiceCallGuard.RequireCommerceCaller(context, InternalScopes.CommerceEligibilityRead, _jwtOptions);

        if (!Guid.TryParse(request.UserId, out var userId) || userId == Guid.Empty)
        {
            throw InternalServiceCallGuard.Error(
                StatusCode.InvalidArgument,
                GrpcCommonErrorCodes.BuyerIdInvalid,
                "USER_ID_INVALID");
        }

        var snapshot = await _queries.GetBuyerCheckoutEligibilityAsync(
            userId,
            request.Address is null
                ? null
                : new BuyerAddressCandidateSnapshot(
                    request.Address.ReceiverName,
                    request.Address.PhoneNumber,
                    request.Address.AddressLine1,
                    request.Address.HasAddressLine2 ? request.Address.AddressLine2 : null,
                    request.Address.Ward,
                    request.Address.District,
                    request.Address.Province,
                    request.Address.CountryCode,
                    request.Address.HasPostalCode ? request.Address.PostalCode : null),
            context.CancellationToken);
        var reply = new BuyerCheckoutEligibilityGrpcReply
        {
            UserId = snapshot.UserId.ToString(),
            AccountStatus = snapshot.AccountStatus,
            PhoneVerified = snapshot.PhoneVerified,
            AddressVerified = snapshot.AddressVerified,
            CanPurchase = snapshot.CanPurchase,
            EligibilityVersion = snapshot.EligibilityVersion,
            CheckedAtUtc = snapshot.CheckedAtUtc.ToString("O")
        };
        reply.Issues.AddRange(snapshot.Issues);
        return reply;
    }

    public override async Task<GetSellerCommerceProfilesGrpcReply> GetSellerCommerceProfiles(
        GetSellerCommerceProfilesGrpcRequest request,
        ServerCallContext context)
    {
        InternalServiceCallGuard.RequireCommerceCaller(context, InternalScopes.CommerceSellerProfileRead, _jwtOptions);

        var ids = ParseIds(request.SellerUserIds);
        var snapshots = await _queries.GetSellerCommerceProfilesAsync(ids, context.CancellationToken);

        var reply = new GetSellerCommerceProfilesGrpcReply();
        reply.Items.AddRange(snapshots.Select(MapSeller));
        return reply;
    }

    private static Guid[] ParseIds(IEnumerable<string> values)
    {
        var ids = new List<Guid>();
        foreach (var value in values)
        {
            if (!Guid.TryParse(value, out var id) || id == Guid.Empty)
            {
                throw InternalServiceCallGuard.Error(
                    StatusCode.InvalidArgument,
                    GrpcCommonErrorCodes.InvalidGuid,
                    "SELLER_USER_ID_INVALID");
            }

            ids.Add(id);
        }

        return ids.ToArray();
    }

    private static SellerCommerceProfileGrpcItem MapSeller(SellerCommerceProfileSnapshot snapshot)
    {
        var item = new SellerCommerceProfileGrpcItem
        {
            RequestedSellerUserId = snapshot.RequestedSellerUserId.ToString(),
            SellerUserId = snapshot.SellerUserId?.ToString() ?? string.Empty,
            ShopName = snapshot.ShopName,
            ChatTargetId = snapshot.ChatTargetId?.ToString() ?? string.Empty,
            SellerStatus = snapshot.SellerStatus,
            ProfileVersion = snapshot.ProfileVersion,
            CanSell = snapshot.CanSell
        };

        if (snapshot.ShopAvatarUrl is { } avatarUrl)
        {
            item.ShopAvatarUrl = avatarUrl;
        }

        item.Issues.AddRange(snapshot.Issues);
        return item;
    }
}
