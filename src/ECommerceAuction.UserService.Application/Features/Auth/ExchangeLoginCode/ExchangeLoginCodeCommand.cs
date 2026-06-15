using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Features.Auth.Common;

namespace ECommerceAuction.UserService.Application.Features.Auth.ExchangeLoginCode;

/// <summary>
/// Exchanges a short-lived OAuth login code for system access and refresh tokens.
/// </summary>
public sealed record ExchangeLoginCodeCommand(string Code) : ICommand<AuthResponse>;
