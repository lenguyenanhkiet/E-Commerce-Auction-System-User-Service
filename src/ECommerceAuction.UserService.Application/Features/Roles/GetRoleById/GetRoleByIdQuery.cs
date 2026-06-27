using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Features.Roles.Common;

namespace ECommerceAuction.UserService.Application.Features.Roles.GetRoleById;

public sealed record GetRoleByIdQuery(Guid RoleId) : IQuery<RoleResponse>;
