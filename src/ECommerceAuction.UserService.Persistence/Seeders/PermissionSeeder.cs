using System.Globalization;
using ECommerceAuction.UserService.Application.Authorization;
using ECommerceAuction.UserService.Domain.Roles;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Seeders;

/// <summary>
/// Seeds the four SRS system roles, the Privileges table (from the compile-time catalog declared
/// under <see cref="Permissions"/>, collected via <see cref="PermissionRegistry.GetAll"/>), and
/// the role-to-privilege assignment matrix. Runs on every startup in deployed environments; every
/// step only INSERTs rows that are missing — nothing is ever updated or deleted — so this is safe
/// to run repeatedly and never overwrites privileges an admin has since customized via Role
/// Management.
/// </summary>
public static class PermissionSeeder
{
    /// <summary>
    /// The four fixed system roles from the SRS. Custom roles are created by admins at runtime
    /// via Role Management and are never touched here.
    /// </summary>
    private static readonly (string Code, string Name, string Description)[] SystemRoles =
    [
        ("ADMIN", "Administrator", "Full administrative access."),
        ("SELLER", "Seller", "Verified seller account."),
        ("BUYER", "Buyer", "Default role for a registered account."),
        ("SUPPORT_STAFF", "Support Staff", "Customer support and moderation access."),
    ];

    /// <summary>
    /// Role -> privilege-code assignments, scoped to the privilege codes user-service itself owns
    /// (see <see cref="Permissions"/>). Ported verbatim from the SRS matrix that used to live in
    /// migration 20260621111636_SeedSrsPrivilegeCatalog (see git history commit deb20ab) before
    /// migrations were squashed to a single schema-only InitialCreate with no seed data. Other
    /// microservices own their own domain's permission codes (PRODUCT.*, ORDER.*, AUCTION.*, ...)
    /// and seed their own role matrix the same way — this list does not attempt to cover those.
    /// </summary>
    private static readonly (string RoleCode, string PrivilegeCode)[] RolePrivilegeMatrix =
    [
        ("ADMIN", "AUTH.LOGIN"), ("SELLER", "AUTH.LOGIN"), ("BUYER", "AUTH.LOGIN"), ("SUPPORT_STAFF", "AUTH.LOGIN"),
        ("ADMIN", "AUTH.LOGOUT"), ("SELLER", "AUTH.LOGOUT"), ("BUYER", "AUTH.LOGOUT"), ("SUPPORT_STAFF", "AUTH.LOGOUT"),

        ("ADMIN", "PROFILE.VIEW"), ("SELLER", "PROFILE.VIEW"), ("BUYER", "PROFILE.VIEW"), ("SUPPORT_STAFF", "PROFILE.VIEW"),
        ("ADMIN", "PROFILE.UPDATE"), ("SELLER", "PROFILE.UPDATE"), ("BUYER", "PROFILE.UPDATE"), ("SUPPORT_STAFF", "PROFILE.UPDATE"),
        ("ADMIN", "PROFILE.CHANGE_PASSWORD"), ("SELLER", "PROFILE.CHANGE_PASSWORD"), ("BUYER", "PROFILE.CHANGE_PASSWORD"), ("SUPPORT_STAFF", "PROFILE.CHANGE_PASSWORD"),
        ("ADMIN", "PROFILE.RESET_PASSWORD"), ("SELLER", "PROFILE.RESET_PASSWORD"), ("BUYER", "PROFILE.RESET_PASSWORD"), ("SUPPORT_STAFF", "PROFILE.RESET_PASSWORD"),

        ("ADMIN", "USER.CREATE"),
        ("ADMIN", "USER.UPDATE"), ("SUPPORT_STAFF", "USER.UPDATE"),
        ("ADMIN", "USER.DELETE"),
        ("ADMIN", "USER.VIEW"), ("SUPPORT_STAFF", "USER.VIEW"),
        ("ADMIN", "USER.LIST"), ("SUPPORT_STAFF", "USER.LIST"),
        ("ADMIN", "USER.CHANGE_PASSWORD"),
        ("ADMIN", "USER.REPUTATION.VIEW"), ("SUPPORT_STAFF", "USER.REPUTATION.VIEW"),
        ("ADMIN", "USER.REPUTATION.ADJUST"),

        ("ADMIN", "ROLE.CREATE"),
        ("ADMIN", "ROLE.UPDATE"),
        ("ADMIN", "ROLE.DELETE"),
        ("ADMIN", "ROLE.VIEW"), ("SUPPORT_STAFF", "ROLE.VIEW"),
        ("ADMIN", "ROLE.LIST"), ("SUPPORT_STAFF", "ROLE.LIST"),

        ("ADMIN", "REPUTATION.VIEW"), ("SELLER", "REPUTATION.VIEW"), ("BUYER", "REPUTATION.VIEW"), ("SUPPORT_STAFF", "REPUTATION.VIEW"),
        ("SELLER", "REPUTATION.RATE"), ("BUYER", "REPUTATION.RATE"),
        ("ADMIN", "REPUTATION.PENALTY.VIEW"), ("SUPPORT_STAFF", "REPUTATION.PENALTY.VIEW"),
        ("ADMIN", "REPUTATION.PENALTY.APPLY"),

        ("ADMIN", "AUDIT_LOG.VIEW"),

        ("ADMIN", "SYSTEM.CONFIG.VIEW"),
        ("ADMIN", "SYSTEM.CONFIG.UPDATE"),
        ("ADMIN", "SYSTEM.HEALTH.VIEW"), ("SUPPORT_STAFF", "SYSTEM.HEALTH.VIEW"),

        // Added after the original SRS matrix migration — seller-application review workflow.
        ("ADMIN", "SELLER.APPLICATION.LIST"), ("SUPPORT_STAFF", "SELLER.APPLICATION.LIST"),
        ("ADMIN", "SELLER.APPLICATION.VIEW"), ("SUPPORT_STAFF", "SELLER.APPLICATION.VIEW"),
        ("ADMIN", "SELLER.APPLICATION.APPROVE"), ("SUPPORT_STAFF", "SELLER.APPLICATION.APPROVE"),
        ("ADMIN", "SELLER.APPLICATION.REJECT"), ("SUPPORT_STAFF", "SELLER.APPLICATION.REJECT"),
    ];

    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        await SeedSystemRolesAsync(dbContext, cancellationToken);
        await SeedPrivilegesAsync(dbContext, cancellationToken);
        await ReconcileRoleAssignmentsAsync(dbContext, cancellationToken);
    }

    /// <summary>
    /// Inserts the four SRS system roles if they don't already exist (matched by code).
    /// </summary>
    private static async Task SeedSystemRolesAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var existingCodes = await dbContext.Roles
            .AsNoTracking()
            .Select(role => role.Code)
            .ToListAsync(cancellationToken);

        var existingCodeSet = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

        var missingRoles = SystemRoles
            .Where(role => !existingCodeSet.Contains(role.Code))
            .Select(role => Role.CreateSystemRole(role.Code, role.Name, role.Description))
            .ToList();

        if (missingRoles.Count == 0)
        {
            return;
        }

        await dbContext.Roles.AddRangeAsync(missingRoles, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Inserts catalog permissions that are not yet in the Privileges table.
    /// </summary>
    private static async Task SeedPrivilegesAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var catalog = PermissionRegistry.GetAll();
        if (catalog.Count == 0)
        {
            return;
        }

        var existingCodes = await dbContext.Privileges
            .AsNoTracking()
            .Select(privilege => privilege.Code)
            .ToListAsync(cancellationToken);

        var existingCodeSet = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

        var newPrivileges = catalog
            .Where(definition => !existingCodeSet.Contains(definition.Code))
            .Select(definition => Privilege.CreateSystem(
                definition.Code,
                Humanize(definition.Code),
                definition.Description))
            .ToList();

        if (newPrivileges.Count == 0)
        {
            return;
        }

        await dbContext.Privileges.AddRangeAsync(newPrivileges, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Ensures every system role has every privilege <see cref="RolePrivilegeMatrix"/> grants it.
    /// Only missing assignments are inserted — an admin's manual grants via Role Management, or
    /// any assignment outside this matrix, are never removed.
    /// </summary>
    private static async Task ReconcileRoleAssignmentsAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var roleCodes = RolePrivilegeMatrix.Select(entry => entry.RoleCode).Distinct().ToArray();

        var roles = await dbContext.Roles
            .AsNoTracking()
            .Where(role => roleCodes.Contains(role.Code))
            .ToListAsync(cancellationToken);

        if (roles.Count == 0)
        {
            return;
        }

        var privilegeIdByCode = await dbContext.Privileges
            .AsNoTracking()
            .ToDictionaryAsync(
                privilege => privilege.Code,
                privilege => privilege.Id,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken);

        foreach (var role in roles)
        {
            var assignedPrivilegeIds = await dbContext.RolePrivileges
                .AsNoTracking()
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.PrivilegeId)
                .ToHashSetAsync(cancellationToken);

            var assignmentsToAdd = RolePrivilegeMatrix
                .Where(entry => entry.RoleCode == role.Code)
                .Select(entry => privilegeIdByCode.GetValueOrDefault(entry.PrivilegeCode))
                // Skip codes this service hasn't seeded a Privilege row for (owned by another
                // microservice's own catalog) and codes already assigned to this role.
                .Where(privilegeId => privilegeId != Guid.Empty && !assignedPrivilegeIds.Contains(privilegeId))
                .Distinct()
                .Select(privilegeId => RolePrivilege.Assign(role.Id, privilegeId, null))
                .ToList();

            if (assignmentsToAdd.Count > 0)
            {
                await dbContext.RolePrivileges.AddRangeAsync(assignmentsToAdd, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    /// <summary>
    /// Turns a permission code such as "SELLER.APPLICATION.LIST" into a display name
    /// "Seller Application List" for the permission matrix UI.
    /// </summary>
    private static string Humanize(string code)
    {
        var words = code.Split(['.', '_'], StringSplitOptions.RemoveEmptyEntries);

        return string.Join(
            ' ',
            words.Select(word => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(word.ToLowerInvariant())));
    }
}
