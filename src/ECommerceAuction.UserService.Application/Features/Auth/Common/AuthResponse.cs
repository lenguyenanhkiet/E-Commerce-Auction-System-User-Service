namespace ECommerceAuction.UserService.Application.Features.Auth.Common;

/// <summary>
/// Response returned to the frontend after a successful login-code exchange or refresh-token rotation.
/// </summary>
public sealed record AuthResponse(
    string AccessToken,
    string? RefreshToken,
    DateTime ExpiresAt,
    AuthUserResponse User);

/// <summary>
/// Minimal authenticated user profile embedded in authentication responses.
/// </summary>
public sealed record AuthUserResponse(
    Guid Id,
    string Email,
    string FullName,
    IReadOnlyCollection<string> Roles);
