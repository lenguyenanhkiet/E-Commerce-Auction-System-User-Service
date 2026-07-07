using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Admin.Users.CreateUser;

/// <summary>
/// Creates a new user account by Admin.
/// Identity number is intentionally excluded: it is captured through the dedicated
/// identity-verification flow (which also requires front/back document images).
/// When <see cref="RoleCodes"/> is null or empty the user is assigned the default BUYER role.
/// </summary>
public sealed record CreateUserCommand(
    string Email,
    string PhoneNumber,
    string FullName,
    string Password,
    string? Gender,
    DateOnly? DateOfBirth,
    string? Address,
    IReadOnlyList<string>? RoleCodes = null)
    : ICommand<CreateUserResponse>;

/// <summary>
/// Result returned after an Admin successfully creates a user.
/// </summary>
public sealed record CreateUserResponse(
    Guid Id,
    string Email,
    string PhoneNumber,
    string FullName,
    string Status,
    IReadOnlyList<string> Roles);
