using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Roles;
using ECommerceAuction.UserService.Domain.Sellers.Applications;
using ECommerceAuction.UserService.Domain.Users;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ECommerceAuction.UserService.Persistence.Repositories.Users;

/// <summary>
/// EF Core implementation of the internal read queries. Seller eligibility is derived
/// from the user's account status and an ACTIVE assignment of the seeded Seller role.
/// </summary>
public sealed class InternalUserQueries : IInternalUserQueries
{
    private const string EligibleStatus = "ELIGIBLE";
    private const string IneligibleStatus = "INELIGIBLE";

    private readonly ApplicationDbContext _db;

    public InternalUserQueries(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<SellerEligibilitySnapshot>> GetSellerEligibilityBatchAsync(
        IReadOnlyCollection<Guid> sellerUserIds,
        CancellationToken cancellationToken)
    {
        var ids = sellerUserIds.Distinct().ToArray();

        var rows = await _db.Users
            .AsNoTracking()
            .Where(user => ids.Contains(user.Id))
            .Select(user => new
            {
                user.Id,
                user.Status,
                user.UpdatedAt,
                SellerRoleActive = user.UserRoles.Any(role =>
                    role.Role.Code == RoleCodes.Seller &&
                    role.Status == UserRoleStatuses.Active &&
                    role.RevokedAt == null)
            })
            .ToListAsync(cancellationToken);

        var byId = rows.ToDictionary(row => row.Id);
        var result = new List<SellerEligibilitySnapshot>(ids.Length);

        foreach (var id in ids)
        {
            if (!byId.TryGetValue(id, out var row))
            {
                result.Add(new SellerEligibilitySnapshot(
                    RequestedSellerUserId: id,
                    Found: false,
                    SellerUserId: null,
                    UserStatus: null,
                    Deleted: false,
                    SellerRoleActive: false,
                    CanSell: false,
                    EligibilityStatus: IneligibleStatus,
                    ReasonCode: "USER_NOT_FOUND",
                    SourceVersion: "0",
                    UpdatedAtUtc: DateTimeOffset.UtcNow));
                continue;
            }

            var statusActive = row.Status == UserStatus.Active;
            var canSell = statusActive && row.SellerRoleActive;
            var updatedAt = row.UpdatedAt ?? DateTimeOffset.UtcNow;

            var reasonCode = canSell
                ? null
                : !statusActive
                    ? $"USER_STATUS_{row.Status}"
                    : "SELLER_ROLE_INACTIVE";

            result.Add(new SellerEligibilitySnapshot(
                RequestedSellerUserId: id,
                Found: true,
                SellerUserId: id,
                UserStatus: row.Status,
                Deleted: false,
                SellerRoleActive: row.SellerRoleActive,
                CanSell: canSell,
                EligibilityStatus: canSell ? EligibleStatus : IneligibleStatus,
                ReasonCode: reasonCode,
                SourceVersion: updatedAt.Ticks.ToString(CultureInfo.InvariantCulture),
                UpdatedAtUtc: updatedAt));
        }

        return result;
    }

    public async Task<IReadOnlyList<UserProfileSnapshot>> GetProfilesBatchAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        var ids = userIds.Distinct().ToArray();

        return await _db.Users
            .AsNoTracking()
            .Where(user => ids.Contains(user.Id))
            .Select(user => new UserProfileSnapshot(user.Id, user.FullName, user.AvatarUrl))
            .ToListAsync(cancellationToken);
    }

    public async Task<BuyerCheckoutEligibilitySnapshot> GetBuyerCheckoutEligibilityAsync(
        Guid userId,
        BuyerAddressCandidateSnapshot? address,
        CancellationToken cancellationToken)
    {
        var checkedAtUtc = DateTimeOffset.UtcNow;
        var row = await _db.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new
            {
                user.Id,
                user.Status,
                user.IsPhoneConfirmed,
                user.UpdatedAt,
                Addresses = _db.Addresses
                    .Where(address => address.UserId == user.Id)
                    .Select(address => new StoredAddressCandidate(
                        address.RecipientName,
                        address.RecipientPhone,
                        address.Street,
                        address.Ward,
                        address.Province,
                        address.UpdatedAt,
                        address.CreatedAt))
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return new BuyerCheckoutEligibilitySnapshot(
                userId,
                "NOT_FOUND",
                PhoneVerified: false,
                AddressVerified: false,
                CanPurchase: false,
                EligibilityVersion: 0,
                Issues: ["USER_NOT_FOUND"],
                CheckedAtUtc: checkedAtUtc);
        }

        var issues = new List<string>();
        if (row.Status != UserStatus.Active)
        {
            issues.Add($"USER_STATUS_{row.Status}");
        }

        if (!row.IsPhoneConfirmed)
        {
            issues.Add("BUYER_PHONE_NOT_VERIFIED");
        }

        var addressVerified = address is not null && row.Addresses.Any(stored => AddressMatches(stored, address));
        if (address is null || !addressVerified)
        {
            issues.Add("BUYER_ADDRESS_NOT_VERIFIED");
        }

        var canPurchase =
            row.Status == UserStatus.Active &&
            row.IsPhoneConfirmed &&
            addressVerified;
        var addressVersion = row.Addresses
            .Select(stored => stored.UpdatedAt ?? stored.CreatedAt)
            .DefaultIfEmpty(checkedAtUtc)
            .Max();
        var versionSource = new[] { row.UpdatedAt ?? checkedAtUtc, addressVersion }.Max();

        return new BuyerCheckoutEligibilitySnapshot(
            row.Id,
            row.Status,
            row.IsPhoneConfirmed,
            addressVerified,
            canPurchase,
            versionSource.Ticks,
            issues,
            checkedAtUtc);
    }

    public async Task<IReadOnlyList<SellerCommerceProfileSnapshot>> GetSellerCommerceProfilesAsync(
        IReadOnlyCollection<Guid> sellerUserIds,
        CancellationToken cancellationToken)
    {
        var ids = sellerUserIds.Distinct().ToArray();
        var rows = await _db.Users
            .AsNoTracking()
            .Where(user => ids.Contains(user.Id))
            .Select(user => new
            {
                user.Id,
                user.Status,
                user.AvatarUrl,
                user.UpdatedAt,
                SellerRoleActive = user.UserRoles.Any(role =>
                    role.Role.Code == RoleCodes.Seller &&
                    role.Status == UserRoleStatuses.Active &&
                    role.RevokedAt == null),
                SellerProfile = _db.SellerProfiles
                    .Where(profile => profile.UserId == user.Id)
                    .OrderByDescending(profile => profile.UpdatedAt ?? profile.CreatedAt)
                    .Select(profile => new
                    {
                        profile.BusinessName,
                        profile.Status,
                        profile.UpdatedAt,
                        profile.CreatedAt
                    })
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var byId = rows.ToDictionary(row => row.Id);
        var result = new List<SellerCommerceProfileSnapshot>(ids.Length);

        foreach (var id in ids)
        {
            if (!byId.TryGetValue(id, out var row))
            {
                result.Add(new SellerCommerceProfileSnapshot(
                    RequestedSellerUserId: id,
                    Found: false,
                    SellerUserId: null,
                    ShopName: string.Empty,
                    ShopAvatarUrl: null,
                    ChatTargetId: null,
                    SellerStatus: "NOT_FOUND",
                    ProfileVersion: 0,
                    CanSell: false,
                    Issues: ["SELLER_NOT_FOUND"]));
                continue;
            }

            var issues = new List<string>();
            if (row.Status != UserStatus.Active)
            {
                issues.Add($"USER_STATUS_{row.Status}");
            }

            if (row.SellerProfile is null)
            {
                issues.Add("SELLER_PROFILE_NOT_FOUND");
            }
            else if (row.SellerProfile.Status != SellerApplicationStatus.Approved)
            {
                issues.Add($"SELLER_STATUS_{row.SellerProfile.Status.ToUpperInvariant()}");
            }

            if (!row.SellerRoleActive)
            {
                issues.Add("SELLER_ROLE_INACTIVE");
            }

            var canSell = row.Status == UserStatus.Active &&
                          row.SellerProfile?.Status == SellerApplicationStatus.Approved &&
                          row.SellerRoleActive;
            var versionSource = row.SellerProfile?.UpdatedAt ??
                                row.SellerProfile?.CreatedAt ??
                                row.UpdatedAt ??
                                DateTimeOffset.UtcNow;

            result.Add(new SellerCommerceProfileSnapshot(
                RequestedSellerUserId: id,
                Found: true,
                SellerUserId: id,
                ShopName: row.SellerProfile?.BusinessName ?? string.Empty,
                ShopAvatarUrl: row.AvatarUrl,
                ChatTargetId: id,
                SellerStatus: row.SellerProfile?.Status ?? "MISSING",
                ProfileVersion: versionSource.Ticks,
                CanSell: canSell,
                Issues: issues));
        }

        return result;
    }

    private static bool AddressMatches(StoredAddressCandidate stored, BuyerAddressCandidateSnapshot candidate)
    {
        return string.Equals(NormalizeText(stored.RecipientName), NormalizeText(candidate.ReceiverName), StringComparison.OrdinalIgnoreCase) &&
               string.Equals(NormalizePhone(stored.RecipientPhone), NormalizePhone(candidate.PhoneNumber), StringComparison.Ordinal) &&
               string.Equals(NormalizeText(stored.Street), NormalizeText(candidate.AddressLine1), StringComparison.OrdinalIgnoreCase) &&
               string.Equals(NormalizeText(stored.Ward), NormalizeText(candidate.Ward), StringComparison.OrdinalIgnoreCase) &&
               string.Equals(NormalizeText(stored.Province), NormalizeText(candidate.Province), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeText(string? value) =>
        string.Join(' ', (value ?? string.Empty).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private static string NormalizePhone(string? value)
    {
        var chars = (value ?? string.Empty).Where(char.IsDigit).ToArray();
        return new string(chars);
    }

    private sealed record StoredAddressCandidate(
        string RecipientName,
        string RecipientPhone,
        string Street,
        string? Ward,
        string? Province,
        DateTimeOffset? UpdatedAt,
        DateTimeOffset CreatedAt);
}
