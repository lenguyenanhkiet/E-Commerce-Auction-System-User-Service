using ECommerceAuction.Contracts.User.Grpc;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Infrastructure.Authentication;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace ECommerceAuction.UserService.Api.GrpcServices;

/// <summary>
/// Internal gRPC service exposing seller-eligibility snapshots to the Catalog Service.
/// Requires a service token (ServiceJwt scheme) carrying the seller-eligibility scope.
/// </summary>
[Authorize(AuthenticationSchemes = ServiceAuthSchemes.ServiceJwt)]
public sealed class UserSellerEligibilityGrpcService
    : UserSellerEligibilityGrpc.UserSellerEligibilityGrpcBase
{
    private readonly IInternalUserQueries _queries;

    public UserSellerEligibilityGrpcService(IInternalUserQueries queries)
    {
        _queries = queries;
    }

    public override async Task<SellerEligibilityBatchReply> GetSellerEligibilityBatch(
        SellerEligibilityBatchRequest request,
        ServerCallContext context)
    {
        InternalServiceCallGuard.RequireScope(context, InternalScopes.SellerEligibilityRead);

        var ids = ParseIds(request.SellerUserIds);
        var snapshots = await _queries.GetSellerEligibilityBatchAsync(ids, context.CancellationToken);

        var reply = new SellerEligibilityBatchReply();
        reply.Items.AddRange(snapshots.Select(Map));
        return reply;
    }

    private static Guid[] ParseIds(IEnumerable<string> values)
    {
        var ids = new List<Guid>();
        foreach (var value in values)
        {
            if (!Guid.TryParse(value, out var id) || id == Guid.Empty)
            {
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument, "SELLER_USER_ID_INVALID"));
            }

            ids.Add(id);
        }

        return ids.ToArray();
    }

    private static SellerEligibilityItem Map(SellerEligibilitySnapshot snapshot)
    {
        var item = new SellerEligibilityItem
        {
            RequestedSellerUserId = snapshot.RequestedSellerUserId.ToString(),
            Found = snapshot.Found,
            Deleted = snapshot.Deleted,
            SellerRoleActive = snapshot.SellerRoleActive,
            CanSell = snapshot.CanSell,
            EligibilityStatus = snapshot.EligibilityStatus,
            SourceVersion = snapshot.SourceVersion,
            UpdatedAtUtc = Timestamp.FromDateTime(
                DateTime.SpecifyKind(snapshot.UpdatedAtUtc, DateTimeKind.Utc))
        };

        if (snapshot.SellerUserId is { } sellerUserId)
        {
            item.SellerUserId = sellerUserId.ToString();
        }

        if (snapshot.UserStatus is { } userStatus)
        {
            item.UserStatus = userStatus;
        }

        if (snapshot.ReasonCode is { } reasonCode)
        {
            item.ReasonCode = reasonCode;
        }

        return item;
    }
}
