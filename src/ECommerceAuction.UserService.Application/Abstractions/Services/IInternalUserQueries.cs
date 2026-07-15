namespace ECommerceAuction.UserService.Application.Abstractions.Services;

/// <summary>
/// Read-side queries that back the internal service-to-service gRPC contracts
/// consumed by the Catalog Service (seller eligibility and user profiles).
/// </summary>
public interface IInternalUserQueries
{
    Task<IReadOnlyList<SellerEligibilitySnapshot>> GetSellerEligibilityBatchAsync(
        IReadOnlyCollection<Guid> sellerUserIds,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<UserProfileSnapshot>> GetProfilesBatchAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken);
}

public sealed record SellerEligibilitySnapshot(
    Guid RequestedSellerUserId,
    bool Found,
    Guid? SellerUserId,
    string? UserStatus,
    bool Deleted,
    bool SellerRoleActive,
    bool CanSell,
    string EligibilityStatus,
    string? ReasonCode,
    string SourceVersion,
    DateTime UpdatedAtUtc);

public sealed record UserProfileSnapshot(
    Guid UserId,
    string FullName,
    string? AvatarUrl);
