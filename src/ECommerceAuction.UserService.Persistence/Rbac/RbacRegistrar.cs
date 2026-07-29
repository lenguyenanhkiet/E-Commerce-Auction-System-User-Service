using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Roles;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ECommerceAuction.UserService.Persistence.Rbac;

public sealed class RbacRegistrar : IRbacRegistrar
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RbacRegistrar> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public RbacRegistrar(ApplicationDbContext context, ILogger<RbacRegistrar> logger, IUnitOfWork unitOfWork)
    {
        _context = context;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task RegisterAsync(RbacRegistrationRequest request, CancellationToken cancellationToken)
    {
        var newPrivileges = await UpsertPrivilegesAsync(request, cancellationToken);
        var newGrants = await UpsertRoleGrantsAsync(request, cancellationToken);

        _logger.LogInformation(
     "RBAC registration from '{ServiceCode}': {NewPrivileges} new privilege(s), {NewGrants} new grant(s).",
     request.ServiceCode, newPrivileges, newGrants);
    }

    /// <summary>
    /// Inserts privilege codes that do not exist yet. Returns how many were added.
    /// </summary>

    private async Task<int> UpsertPrivilegesAsync(RbacRegistrationRequest request, CancellationToken cancellationToken)

    {
        if (request.Privileges.Count == 0)
        {
            return 0;
        }

        var existingCodes = await _context.Privileges.Select(privilege => privilege.Code).ToListAsync(cancellationToken);

        var known = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

        var toAdd = new List<Privilege>();
        foreach (var definition in request.Privileges)
        {
            var code = Normalize(definition.Code);
            if (code.Length == 0)
            {
                continue;
            }

            // `known` also guards against the same code appearing twice in one request.

            if (!known.Add(code))
            {
                continue;
            }

            toAdd.Add(Privilege.CreateSystem(code, Humanize(code), definition.Description));
        }
        if (toAdd.Count == 0)
        {
            return 0;
        }

        _context.Privileges.AddRange(toAdd);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return toAdd.Count;
    }

    /// <summary>
    /// Assigns privileges to roles. Runs after privileges are saved so their ids exist.
    /// A grant referencing an unknownn role or privilege is skipped and logged rather than
    /// throwing: one bad line must not abort a service's whole registration.
    /// </summary>

    private async Task<int> UpsertRoleGrantsAsync(RbacRegistrationRequest request, CancellationToken cancellationToken)
    {
        if (request.RoleGrants.Count == 0)
        {
            return 0;
        }

        var privilegeIdByCode = await _context.Privileges.ToDictionaryAsync(privilege => privilege.Code, privilege => privilege.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var roleIdByCode = (await _context.Roles.Where(role => role.DeletedAt == null).ToDictionaryAsync(role => role.Code, role => role.Id, StringComparer.OrdinalIgnoreCase, cancellationToken));

        var existingPairs = (await _context.RolePrivileges.Select(rolePrivilege => new { rolePrivilege.RoleId, rolePrivilege.PrivilegeId }).ToListAsync(cancellationToken)).Select(pair => (pair.RoleId, pair.PrivilegeId)).ToHashSet();

        var toAdd = new List<RolePrivilege>();
        foreach (var grant in request.RoleGrants)
        {
            var roleCode = Normalize(grant.RoleCode);
            var privilegeCode = Normalize(grant.PrivilegeCode);

            if (!roleIdByCode.TryGetValue(roleCode, out var roleId))
            {
                _logger.LogWarning($"Skipping grant from {request.ServiceCode}: unknown role {grant.RoleCode}.");
                continue;
            }

            if (!privilegeIdByCode.TryGetValue(privilegeCode, out var privilegeId))
            {
                _logger.LogWarning($"Skipping grant from {request.ServiceCode}: unknown privilege {grant.PrivilegeCode}.");
                continue;
            }

            // Also dedupes repeats inside a single request.

            if (!existingPairs.Add((roleId, privilegeId)))
            {
                continue;
            }

            // assignedBy is null: this is a system registration, not an administrator action.
            toAdd.Add(RolePrivilege.Assign(roleId, privilegeId, null));
        }

        if (toAdd.Count == 0)
        {
            return 0;
        }

        _context.RolePrivileges.AddRange(toAdd);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return toAdd.Count;
    }

    private static string Normalize(string? value) =>
       (value ?? string.Empty).Trim().ToUpperInvariant();

    /// <summary>
    /// Turns a code into a readable name: "AUCTION.VIEW_BID_HISTORY" -> "Auction View Bid History".
    /// </summary>
    private static string Humanize(string code)
    {
        var words = code.Split(['.', '_'], StringSplitOptions.RemoveEmptyEntries);
        var titleCase = CultureInfo.InvariantCulture.TextInfo;

        return string.Join(' ', words.Select(word => titleCase.ToTitleCase(word.ToLowerInvariant())));
    }
}