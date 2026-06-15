using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Domain.Repositories;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Entities.Users;

namespace ECommerceAuction.UserService.Application.Features.Auth.RegisterAccount;

public class RegisterAccountCommandHandler
    : ICommandHandler<RegisterAccountCommand, RegisterAccountResponse>
{
    public Task<RegisterAccountResponse> Handle(RegisterAccountCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}