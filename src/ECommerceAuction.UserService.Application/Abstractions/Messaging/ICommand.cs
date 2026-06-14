using MediatR;

namespace ECommerceAuction.UserService.Application.Abstractions.Messaging;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

