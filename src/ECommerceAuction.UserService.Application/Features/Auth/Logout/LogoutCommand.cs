using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Auth.Logout;

/// <summary>
/// Logs out the current authenticated user and optionally revokes the supplied refresh token session.
/// </summary>
public sealed record LogoutCommand(string? RefreshToken) : ICommand<LogoutResponse>;

/// <summary>
/// Response returned after logout completes successfully.
/// </summary>
public sealed record LogoutResponse(
    bool Success,
    string Message,
    DateTimeOffset LoggedOutAt);
