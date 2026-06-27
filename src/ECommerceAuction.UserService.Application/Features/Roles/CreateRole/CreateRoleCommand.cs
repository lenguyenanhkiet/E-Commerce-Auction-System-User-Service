using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Features.Roles.Common;

namespace ECommerceAuction.UserService.Application.Features.Roles.CreateRole;

public sealed record CreateRoleCommand(
    string Code,
    string Name,
    string? Description,
    IReadOnlyCollection<string> PrivilegeCodes) : ICommand<RoleResponse>;
