namespace ECommerceAuction.UserService.Application.Features.Roles.Common;

public sealed record PrivilegeResponse(Guid Id, string Code, string Name, string? Description);

public sealed record RoleResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsSystemRole,
    string Status,
    IReadOnlyCollection<PrivilegeResponse> Privileges);
