using ECommerceAuction.Contracts.User.Grpc;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Infrastructure.Authentication;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace ECommerceAuction.UserService.Api.GrpcServices;

/// <summary>
/// Internal gRPC service exposing public user display profiles to the Catalog Service.
/// </summary>
[Authorize(AuthenticationSchemes = ServiceAuthSchemes.ServiceJwt)]
public sealed class UserProfileGrpcService
    : UserProfileGrpc.UserProfileGrpcBase
{
    private readonly IInternalUserQueries _queries;

    public UserProfileGrpcService(IInternalUserQueries queries)
    {
        _queries = queries;
    }

    public override async Task<UserProfileBatchReply> GetProfilesBatch(
        UserProfileBatchRequest request,
        ServerCallContext context)
    {
        // The Catalog client currently obtains a single internal-read scope for all
        // User Service calls, so profile reads accept the same seller-eligibility scope.
        // Split into InternalScopes.UserProfileRead once the client requests it per call.
        InternalServiceCallGuard.RequireScope(context, InternalScopes.SellerEligibilityRead);

        var ids = ParseIds(request.UserIds);
        var snapshots = await _queries.GetProfilesBatchAsync(ids, context.CancellationToken);

        var reply = new UserProfileBatchReply();
        reply.Items.AddRange(snapshots.Select(snapshot =>
        {
            var item = new UserProfileItem
            {
                UserId = snapshot.UserId.ToString(),
                FullName = snapshot.FullName
            };

            if (snapshot.AvatarUrl is { } avatarUrl)
            {
                item.AvatarUrl = avatarUrl;
            }

            return item;
        }));

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
                    StatusCode.InvalidArgument, "USER_ID_INVALID"));
            }

            ids.Add(id);
        }

        return ids.ToArray();
    }
}
