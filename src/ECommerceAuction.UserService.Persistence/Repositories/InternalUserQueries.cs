using System.Globalization;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Entities.Users;
using ECommerceAuction.UserService.Persistence.Context;
using ECommerceAuction.UserService.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Repositories;

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
                    role.RoleId == RbacSeedData.SellerRoleId &&
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
                    UpdatedAtUtc: DateTime.UtcNow));
                continue;
            }

            var statusActive = row.Status == UserStatus.Active;
            var canSell = statusActive && row.SellerRoleActive;
            var updatedAt = row.UpdatedAt ?? DateTime.UtcNow;

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
}
