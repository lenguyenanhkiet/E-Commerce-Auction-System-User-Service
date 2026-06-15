using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Auth.RegisterAccount;

public record RegisterAccountCommand(
    string Email,
    string PhoneNumber,
    string FullName,
    string Password
) : ICommand<RegisterAccountResponse>;