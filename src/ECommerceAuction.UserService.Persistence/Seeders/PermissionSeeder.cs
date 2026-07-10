using System.Globalization;
using ECommerceAuction.UserService.Application.Authorization;
using ECommerceAuction.UserService.Domain.Entities.Roles;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Seeders;

/// <summary>
/// Seeds the Privileges table from the compile-time permission catalog declared under
/// <see cref="Permissions"/> (collected via <see cref="PermissionRegistry.GetAll"/>).
/// Only codes missing from the database are inserted — existing rows (including any status
/// an admin has manually changed) are left untouched, so this is safe to run on every startup.
/// Also auto-assigns new permissions to system roles based on their scope.
/// </summary>
public static class PermissionSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        var catalog = PermissionRegistry.GetAll();
        if (catalog.Count == 0)
        {
            return;
        }

        var existingPrivileges = await dbContext.Privileges
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var existingCodeSet = new HashSet<string>(
            existingPrivileges.Select(p => p.Code),
            StringComparer.OrdinalIgnoreCase);

        // 1. Insert any catalog permissions that are not yet in the Privileges table.
        var newPrivileges = catalog
            .Where(definition => !existingCodeSet.Contains(definition.Code))
            .Select(definition => Privilege.CreateSystem(
                definition.Code,
                Humanize(definition.Code),
                definition.Description))
            .ToList();

        if (newPrivileges.Count > 0)
        {
            await dbContext.Privileges.AddRangeAsync(newPrivileges, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // 2. Reconcile role → privilege assignments for system roles. This runs on EVERY startup,
        //    even when no new privileges were inserted, so that permissions which already existed
        //    in the Privileges table but were never assigned to a role get back-filled.
        await ReconcileRoleAssignmentsAsync(dbContext, cancellationToken);
    }

    /// <summary>
    /// Ensures ADMIN and SUPPORT_STAFF have every privilege their scope grants
    /// (see <see cref="ShouldAssignToRole"/>). Existing assignments are left untouched;
    /// only missing ones are inserted, so this is idempotent and safe to run each startup.
    /// </summary>
    private static async Task ReconcileRoleAssignmentsAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var allPrivileges = await dbContext.Privileges
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var systemRoles = await dbContext.Roles
            .AsNoTracking()
            .Where(r => r.Code == "ADMIN" || r.Code == "SUPPORT_STAFF")
            .ToListAsync(cancellationToken);

        foreach (var role in systemRoles)
        {
            var assignedPrivilegeIds = await dbContext.RolePrivileges
                .AsNoTracking()
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.PrivilegeId)
                .ToHashSetAsync(cancellationToken);

            var assignmentsToAdd = allPrivileges
                .Where(privilege =>
                    !assignedPrivilegeIds.Contains(privilege.Id) &&
                    ShouldAssignToRole(privilege.Code, role.Code))
                .Select(privilege => RolePrivilege.Assign(role.Id, privilege.Id, null))
                .ToList();

            if (assignmentsToAdd.Count > 0)
            {
                await dbContext.RolePrivileges.AddRangeAsync(assignmentsToAdd, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    /// <summary>
    /// Determines if a permission should be automatically assigned to a given role.
    /// This back-fills ONLY permissions added to the catalog after the SRS matrix migration
    /// (currently the SELLER.APPLICATION.* review permissions). The migration owns the rest of
    /// the matrix, so we must NOT prefix-grant whole groups like USER.* or ROLE.* to
    /// SUPPORT_STAFF — that would over-grant destructive permissions (USER.DELETE, ROLE.DELETE)
    /// the SRS deliberately withholds from support staff.
    /// </summary>
    private static bool ShouldAssignToRole(string permissionCode, string roleCode)
    {
        // Seller-application review: ADMIN gets all four, SUPPORT_STAFF handles the workflow.
        if (permissionCode.StartsWith("SELLER.APPLICATION."))
        {
            return roleCode is "ADMIN" or "SUPPORT_STAFF";
        }

        // ADMIN is the catch-all administrator: grant any other newly-added catalog permission
        // except end-user-only groups. SUPPORT_STAFF gets nothing beyond what the migration set.
        return roleCode == "ADMIN"
               && !permissionCode.StartsWith("BUYER.")
               && !permissionCode.StartsWith("CART.")
               && !permissionCode.StartsWith("CHECKOUT.")
               && !permissionCode.StartsWith("PROFILE.");
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
