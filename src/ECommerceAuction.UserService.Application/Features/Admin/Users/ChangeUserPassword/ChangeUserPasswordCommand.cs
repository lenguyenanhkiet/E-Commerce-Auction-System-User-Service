using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Admin.Users.ChangeUserPassword;

/// <summary>
/// Changes another user's password by Admin.
/// </summary>
public sealed record ChangeUserPasswordCommand(
    Guid UserId,
    string NewPassword,
    bool RequireChangeOnNextLogin = true)
    : ICommand<ChangeUserPasswordResponse>;

/// <summary>
/// Result returned after an Admin resets a user's password.
/// </summary>
public sealed record ChangeUserPasswordResponse(
    Guid Id,
    DateTimeOffset ChangedAt);
