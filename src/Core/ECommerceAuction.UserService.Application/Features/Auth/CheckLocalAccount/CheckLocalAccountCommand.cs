using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Auth.CheckLocalAccount;

public record CheckLocalAccountCommand(
	string EmailOrPhone,
	string Password
) : ICommand<CheckLocalAccountResponse>;