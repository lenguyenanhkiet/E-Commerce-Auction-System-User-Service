using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Features.Roles.Common;

namespace ECommerceAuction.UserService.Application.Features.Roles.GetRoles;

public sealed record GetRolesQuery(
    string? Search,
    string? Status,
    string? SortBy,
    string? SortDirection) : IQuery<IReadOnlyCollection<RoleResponse>>;
