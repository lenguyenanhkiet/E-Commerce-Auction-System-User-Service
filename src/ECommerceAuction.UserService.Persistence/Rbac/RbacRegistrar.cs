using System.Globalization;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Entities.Roles;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerceAuction.UserService.Persistence.Rbac;

/// <summary>
/// Persists privilege definitions and default grants submitted by business services.
/// Registration is insert-only and idempotent.
/// </summary>
public sealed class RbacRegistrar : IRbacRegistrar
{
    private static readonly HashSet<string> AllowedSystemRoleCodes = new(
        [
            RoleCodes.Admin,
            RoleCodes.Buyer,
            RoleCodes.Seller,
            RoleCodes.SupportStaff
        ],
        StringComparer.OrdinalIgnoreCase);

    private readonly ApplicationDbContext _context;
    private readonly ILogger<RbacRegistrar> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public RbacRegistrar(
        ApplicationDbContext context,
        ILogger<RbacRegistrar> logger,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task RegisterAsync(
        RbacRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var newPrivileges = await UpsertPrivilegesAsync(
            request,
            cancellationToken);

        var newGrants = await UpsertRoleGrantsAsync(
            request,
            cancellationToken);

        _logger.LogInformation(
            "RBAC registration from '{ServiceCode}': {NewPrivileges} new privilege(s), {NewGrants} new grant(s).",
            request.ServiceCode,
            newPrivileges,
            newGrants);
    }

    /// <summary>
    /// Inserts privilege codes that do not exist yet and returns how many were added.
    /// </summary>
    private async Task<int> UpsertPrivilegesAsync(
        RbacRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Privileges.Count == 0)
        {
            return 0;
        }

        var existingCodes = await _context.Privileges
            .AsNoTracking()
            .Select(privilege => privilege.Code)
            .ToListAsync(cancellationToken);

        var knownCodes = new HashSet<string>(
            existingCodes,
            StringComparer.OrdinalIgnoreCase);

        var privilegesToAdd = new List<Privilege>();

        foreach (var definition in request.Privileges)
        {
            var code = Normalize(definition.Code);
            if (code.Length == 0 || !knownCodes.Add(code))
            {
                continue;
            }

            privilegesToAdd.Add(
                Privilege.CreateSystem(
                    code,
                    Humanize(code),
                    definition.Description));
        }

        if (privilegesToAdd.Count == 0)
        {
            return 0;
        }

        await _context.Privileges.AddRangeAsync(
            privilegesToAdd,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return privilegesToAdd.Count;
    }

    /// <summary>
    /// Assigns registered privileges only to the four protected system roles.
    /// Grants targeting custom, deleted, unknown, or non-system roles are ignored and logged.
    /// </summary>
    private async Task<int> UpsertRoleGrantsAsync(
        RbacRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        if (request.RoleGrants.Count == 0)
        {
            return 0;
        }

        var privilegeIdByCode = await _context.Privileges
            .AsNoTracking()
            .ToDictionaryAsync(
                privilege => privilege.Code,
                privilege => privilege.Id,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken);

        var roles = await _context.Roles
            .AsNoTracking()
            .Where(role =>
                role.DeletedAt == null &&
                role.IsSystemRole &&
                AllowedSystemRoleCodes.Contains(role.Code))
            .ToListAsync(cancellationToken);

        var roleIdByCode = roles.ToDictionary(
            role => role.Code,
            role => role.Id,
            StringComparer.OrdinalIgnoreCase);

        var existingPairs = (await _context.RolePrivileges
                .AsNoTracking()
                .Select(rolePrivilege => new
                {
                    rolePrivilege.RoleId,
                    rolePrivilege.PrivilegeId
                })
                .ToListAsync(cancellationToken))
            .Select(pair => (pair.RoleId, pair.PrivilegeId))
            .ToHashSet();

        var grantsToAdd = new List<RolePrivilege>();

        foreach (var grant in request.RoleGrants)
        {
            var roleCode = Normalize(grant.RoleCode);
            var privilegeCode = Normalize(grant.PrivilegeCode);

            if (!AllowedSystemRoleCodes.Contains(roleCode))
            {
                _logger.LogWarning(
                    "Skipping RBAC grant from '{ServiceCode}': role '{RoleCode}' is not an allowed system role.",
                    request.ServiceCode,
                    grant.RoleCode);

                continue;
            }

            if (!roleIdByCode.TryGetValue(roleCode, out var roleId))
            {
                _logger.LogWarning(
                    "Skipping RBAC grant from '{ServiceCode}': system role '{RoleCode}' does not exist or is inactive.",
                    request.ServiceCode,
                    grant.RoleCode);

                continue;
            }

            if (!privilegeIdByCode.TryGetValue(
                    privilegeCode,
                    out var privilegeId))
            {
                _logger.LogWarning(
                    "Skipping RBAC grant from '{ServiceCode}': privilege '{PrivilegeCode}' does not exist.",
                    request.ServiceCode,
                    grant.PrivilegeCode);

                continue;
            }

            if (!existingPairs.Add((roleId, privilegeId)))
            {
                continue;
            }

            grantsToAdd.Add(
                RolePrivilege.Assign(
                    roleId,
                    privilegeId,
                    assignedBy: null));
        }

        if (grantsToAdd.Count == 0)
        {
            return 0;
        }

        await _context.RolePrivileges.AddRangeAsync(
            grantsToAdd,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return grantsToAdd.Count;
    }

    private static string Normalize(string? value) =>
        (value ?? string.Empty).Trim().ToUpperInvariant();

    /// <summary>
    /// Turns a code such as AUCTION.VIEW_BID_HISTORY into Auction View Bid History.
    /// </summary>
    private static string Humanize(string code)
    {
        var words = code.Split(
            ['.', '_'],
            StringSplitOptions.RemoveEmptyEntries);

        var titleCase = CultureInfo.InvariantCulture.TextInfo;

        return string.Join(
            ' ',
            words.Select(word =>
                titleCase.ToTitleCase(word.ToLowerInvariant())));
    }
}
