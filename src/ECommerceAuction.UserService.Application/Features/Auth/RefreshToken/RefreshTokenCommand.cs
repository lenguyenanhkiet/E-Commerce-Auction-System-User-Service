using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Features.Auth.Common;

namespace ECommerceAuction.UserService.Application.Features.Auth.RefreshToken;

/// <summary>
/// Rotates an active refresh token and returns a new access token and refresh token.
/// </summary>
public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AuthResponse>;
