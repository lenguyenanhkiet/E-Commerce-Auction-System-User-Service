using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Admin.Users.GetUsers;

/// <summary>
/// Gets a paginated and filtered list of users for Admin.
/// </summary>
public sealed record GetUsersQuery(
    string? Search,
    string? Gender,
    string? Status,
    int Page = 1,
    int PageSize = 20)
    : IQuery<PagedUsersResponse>;

/// <summary>
/// User information displayed on the Admin management page.
/// </summary>
public sealed record AdminUserItem(
    Guid Id,
    string FullName,
    string Email,
    string PhoneNumber,
    string? IdentityNumber,
    string? Gender,
    string? Address,
    DateOnly? DateOfBirth);

/// <summary>
/// Paginated API response.
/// </summary>
public sealed record PagedUsersResponse(
    IReadOnlyList<AdminUserItem> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);