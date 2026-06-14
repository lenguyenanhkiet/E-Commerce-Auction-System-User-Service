namespace ECommerceAuction.UserService.Application.Abstractions.Services;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent eventMessage, CancellationToken cancellationToken = default)
        where TEvent : class;
}

