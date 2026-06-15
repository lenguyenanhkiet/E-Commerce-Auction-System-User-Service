using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Repositories;

namespace ECommerceAuction.UserService.Application.Features.Auth.CheckLocalAccount;

public class CheckLocalAccountCommandHandler
    : ICommandHandler<CheckLocalAccountCommand, CheckLocalAccountResponse>
{
    public Task<CheckLocalAccountResponse> Handle(CheckLocalAccountCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}