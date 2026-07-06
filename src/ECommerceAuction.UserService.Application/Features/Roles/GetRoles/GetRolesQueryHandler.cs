using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Features.Roles.Common;
using ECommerceAuction.UserService.Domain.Entities.Roles;

namespace ECommerceAuction.UserService.Application.Features.Roles.GetRoles;

/// <summary>
/// Role Management: searches, filters, and sorts all non-deleted roles.
/// </summary>
public sealed class GetRolesQueryHandler
    : IQueryHandler<GetRolesQuery, IReadOnlyCollection<RoleResponse>>
{
    private readonly IRoleManagementRepository _repository;

    public GetRolesQueryHandler(IRoleManagementRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<RoleResponse>> Handle(
        GetRolesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _repository.QueryRoles()
            .Where(role => role.Status != RoleStatuses.Deleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim();

            // Search covers the fields shown in the management list screen.
            query = query.Where(role =>
                role.Code.Contains(keyword) ||
                role.Name.Contains(keyword) ||
                (role.Description != null && role.Description.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim().ToUpperInvariant();

            // Status filter lets admins inspect active or inactive roles without exposing deleted roles.
            query = query.Where(role => role.Status == status);
        }

        query = ApplySorting(query, request.SortBy, request.SortDirection);

        var roles = await _repository.ListRolesAsync(query, cancellationToken);

        return roles.Select(RoleMapper.ToResponse).ToArray();
    }

    private static IQueryable<Role> ApplySorting(
        IQueryable<Role> query,
        string? sortBy,
        string? sortDirection)
    {
        var descending = string.Equals(
            sortDirection,
            "desc",
            StringComparison.OrdinalIgnoreCase);

        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "code" => descending
                ? query.OrderByDescending(role => role.Code)
                : query.OrderBy(role => role.Code),
            "createdat" => descending
                ? query.OrderByDescending(role => role.CreatedAt)
                : query.OrderBy(role => role.CreatedAt),
            "updatedat" => descending
                ? query.OrderByDescending(role => role.UpdatedAt)
                : query.OrderBy(role => role.UpdatedAt),
            _ => descending
                ? query.OrderByDescending(role => role.Name)
                : query.OrderBy(role => role.Name)
        };
    }
}
