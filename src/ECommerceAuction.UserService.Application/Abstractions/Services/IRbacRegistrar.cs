namespace ECommerceAuction.UserService.Application.Abstractions.Services;

/// <summary>
/// Lets another microservice register the privilege codes it owns (e.g. Catalog's
/// CATEGORY.*/PRODUCT.*) and the role grants for them into the shared RBAC store,
/// so User Service can mint tokens carrying those privileges. Idempotent: only
/// missing privileges and assignments are inserted.
/// </summary>
public interface IRbacRegistrar
{
    Task RegisterAsync(RbacRegistrationRequest request, CancellationToken cancellationToken);
}

public sealed record RbacRegistrationRequest(
    string ServiceCode,
    IReadOnlyList<PrivilegeDefinition> Privileges,
    IReadOnlyList<RoleGrant> RoleGrants);

public sealed record PrivilegeDefinition(string Code, string? Description);

public sealed record RoleGrant(string RoleCode, string PrivilegeCode);
