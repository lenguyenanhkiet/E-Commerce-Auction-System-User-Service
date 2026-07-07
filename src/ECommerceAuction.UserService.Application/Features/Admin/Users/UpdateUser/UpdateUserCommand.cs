using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Admin.Users.UpdateUser;

/// <summary>
/// Updates an existing user's information by Admin.
/// When <see cref="RoleCodes"/> is null the user's roles are left unchanged; when it is
/// provided the active roles are reconciled to exactly match the supplied set.
/// </summary>
public sealed record UpdateUserCommand(
    Guid UserId,
    string FullName,
    string? Email,
    string? Gender,
    DateOnly? DateOfBirth,
    string? Address,
    IReadOnlyList<string>? RoleCodes = null)
    : ICommand<UpdateUserResponse>;

/// <summary>
/// Result returned after an Admin successfully updates a user.
/// </summary>
public sealed record UpdateUserResponse(
    Guid Id,
    bool IsUpdated,
    IReadOnlyList<string> Roles);
