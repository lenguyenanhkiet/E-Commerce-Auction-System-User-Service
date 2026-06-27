using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Features.Roles.Common;

namespace ECommerceAuction.UserService.Application.Features.Roles.UpdateRole;

public sealed record UpdateRoleCommand(
    Guid RoleId,
    string Name,
    string? Description,
    IReadOnlyCollection<string> PrivilegeCodes) : ICommand<RoleResponse>;

public sealed record UpdateRoleRequest(
    string Name,
    string? Description,
    IReadOnlyCollection<string> PrivilegeCodes);
