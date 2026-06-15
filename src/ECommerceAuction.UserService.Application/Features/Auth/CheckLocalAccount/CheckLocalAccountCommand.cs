using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Features.Auth.Common;

namespace ECommerceAuction.UserService.Application.Features.Auth.CheckLocalAccount;

public record CheckLocalAccountCommand(
	string EmailOrPhone,
	string Password
) : ICommand<AuthResponse>;
