using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Features.Roles.Common;

namespace ECommerceAuction.UserService.Application.Features.Roles.GetPrivileges;

public sealed record GetPrivilegesQuery : IQuery<IReadOnlyCollection<PrivilegeResponse>>;
