namespace ECommerceAuction.UserService.Application.Features.Auth.Common;

/// <summary>
/// Response returned to the frontend after a successful login-code exchange or refresh-token rotation.
/// </summary>
public sealed record AuthResponse(
    string AccessToken,
    string? RefreshToken,
    DateTimeOffset ExpiresAt,
    string Status,
    bool MustChangePassword = false);