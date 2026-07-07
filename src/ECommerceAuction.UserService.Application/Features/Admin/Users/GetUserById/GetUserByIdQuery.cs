using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Admin.Users.GetUserById;

/// <summary>
/// Gets the full detail of a single user for Admin.
/// </summary>
public sealed record GetUserByIdQuery(Guid UserId) : IQuery<AdminUserDetailResponse>;

/// <summary>
/// Detailed user information returned on the Admin view page.
/// </summary>
public sealed record AdminUserDetailResponse(
    Guid Id,
    string FullName,
    string Email,
    string PhoneNumber,
    string? IdentityNumber,
    string? Gender,
    string? Address,
    DateOnly? DateOfBirth,
    string Status,
    bool IsEmailConfirmed,
    bool IsPhoneConfirmed,
    IReadOnlyList<string> Roles,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
