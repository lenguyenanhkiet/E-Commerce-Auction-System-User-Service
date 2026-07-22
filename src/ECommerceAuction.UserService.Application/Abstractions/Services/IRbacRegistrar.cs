using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Abstractions.Services;
/// <summary>
/// Registers privileges and role grants declared by another service.
/// Implementations MUST be idempotent and insert-only: a service re-registering on every
/// startup must not duplicate rows, and must never remove privileges it no longer declares
/// (other services and existing tokens may still depend on them).
/// </summary>

public interface IRbacRegistrar
{
    Task RegisterAsync(RbacRegistrationRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// What one service declares it owns. <paramref name="ServiceCode"/> is informational
/// (logging / auditing); privileges are global, not namespaced per service.
/// </summary>
public sealed record RbacRegistrationRequest
    (
        string ServiceCode,
        IReadOnlyList<PrivilegeDefinition> Privileges,
        IReadOnlyList<RoleGrant> RoleGrants
    );
/// <summary>
/// A single privilege code, e.g. "CATEGORY.CREATE".
/// </summary>
public sealed record PrivilegeDefinition(string Code, string? Description);
/// <summary>
/// Grants <paramref name="PrivilegeCode"/> to <paramref name="RoleCode"/>, e.g. ADMIN.
/// </summary>
public sealed record RoleGrant(string RoleCode, string PrivilegeCode);