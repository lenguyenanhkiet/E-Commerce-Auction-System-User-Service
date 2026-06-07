using MediatR;

namespace ECommerceAuction.UserService.Application.Abstractions.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}

