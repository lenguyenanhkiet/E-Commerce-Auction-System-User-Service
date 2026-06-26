using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Roles.DeleteRole;

public sealed record DeleteRoleCommand(Guid RoleId) : ICommand;
