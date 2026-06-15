using ECommerceAuction.UserService.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace ECommerceAuction.UserService.Infrastructure.EventBus;

public sealed class RabbitMQEventBus : IEventBus
{
    private readonly ILogger<RabbitMQEventBus> _logger;

    public RabbitMQEventBus(ILogger<RabbitMQEventBus> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent eventMessage, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        _logger.LogInformation("Mock event published: {EventType}", typeof(TEvent).Name);

        return Task.CompletedTask;
    }
}

