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

    Task<BuyerCheckoutEligibilitySnapshot> GetBuyerCheckoutEligibilityAsync(
        Guid userId,
        BuyerAddressCandidateSnapshot? address,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SellerCommerceProfileSnapshot>> GetSellerCommerceProfilesAsync(
        IReadOnlyCollection<Guid> sellerUserIds,
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
    DateTimeOffset UpdatedAtUtc);

public sealed record UserProfileSnapshot(
    Guid UserId,
    string FullName,
    string? AvatarUrl);

public sealed record BuyerAddressCandidateSnapshot(
    string ReceiverName,
    string PhoneNumber,
    string AddressLine1,
    string? AddressLine2,
    string Ward,
    string District,
    string Province,
    string CountryCode,
    string? PostalCode);

public sealed record BuyerCheckoutEligibilitySnapshot(
    Guid UserId,
    string AccountStatus,
    bool PhoneVerified,
    bool AddressVerified,
    bool CanPurchase,
    long EligibilityVersion,
    IReadOnlyCollection<string> Issues,
    DateTimeOffset CheckedAtUtc);

public sealed record SellerCommerceProfileSnapshot(
    Guid RequestedSellerUserId,
    bool Found,
    Guid? SellerUserId,
    string ShopName,
    string? ShopAvatarUrl,
    Guid? ChatTargetId,
    string SellerStatus,
    long ProfileVersion,
    bool CanSell,
    IReadOnlyCollection<string> Issues);
