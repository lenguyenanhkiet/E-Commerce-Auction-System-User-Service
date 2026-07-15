using System.Globalization;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Entities.Roles;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Repositories;

/// <summary>
/// Upserts externally-owned privileges and their role grants into the shared RBAC tables.
/// Only inserts what is missing (matched by code), so it is safe to call on every startup.
/// </summary>
public sealed class RbacRegistrar : IRbacRegistrar
{
    private readonly ApplicationDbContext _db;

    public RbacRegistrar(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task RegisterAsync(RbacRegistrationRequest request, CancellationToken cancellationToken)
    {
        // 1) Insert privileges that don't exist yet (matched by normalized code).
        var existingCodes = await _db.Privileges
            .AsNoTracking()
            .Select(privilege => privilege.Code)
            .ToListAsync(cancellationToken);
        var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

        var newPrivileges = request.Privileges
            .Select(p => (Code: p.Code.Trim().ToUpperInvariant(), p.Description))
            .Where(p => !string.IsNullOrWhiteSpace(p.Code) && !existing.Contains(p.Code))
            .GroupBy(p => p.Code)
            .Select(g => Privilege.CreateSystem(g.Key, Humanize(g.Key), g.First().Description))
            .ToList();

        if (newPrivileges.Count > 0)
        {
            await _db.Privileges.AddRangeAsync(newPrivileges, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        // 2) Insert missing role -> privilege assignments.
        var privilegeIdByCode = await _db.Privileges
            .AsNoTracking()
            .ToDictionaryAsync(p => p.Code, p => p.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var roleIdByCode = await _db.Roles
            .AsNoTracking()
            .ToDictionaryAsync(r => r.Code, r => r.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var toAssign = new List<RolePrivilege>();
        foreach (var grant in request.RoleGrants)
        {
            if (!roleIdByCode.TryGetValue(grant.RoleCode.Trim(), out var roleId) ||
                !privilegeIdByCode.TryGetValue(grant.PrivilegeCode.Trim(), out var privilegeId))
            {
                continue;
            }

            var alreadyAssigned = await _db.RolePrivileges
                .AsNoTracking()
                .AnyAsync(rp => rp.RoleId == roleId && rp.PrivilegeId == privilegeId, cancellationToken);

            if (!alreadyAssigned && toAssign.All(a => !(a.RoleId == roleId && a.PrivilegeId == privilegeId)))
            {
                toAssign.Add(RolePrivilege.Assign(roleId, privilegeId, null));
            }
        }

        if (toAssign.Count > 0)
        {
            await _db.RolePrivileges.AddRangeAsync(toAssign, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private static string Humanize(string code)
    {
        var words = code.Split(['.', '_'], StringSplitOptions.RemoveEmptyEntries);
        return string.Join(
            ' ',
            words.Select(w => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(w.ToLowerInvariant())));
    }
}
