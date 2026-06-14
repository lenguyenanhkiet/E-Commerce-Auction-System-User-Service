using ECommerceAuction.UserService.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace ECommerceAuction.UserService.Infrastructure.EventBus;

public sealed class MockEventBus : IEventBus
{
    private readonly ILogger<MockEventBus> _logger;

    public MockEventBus(ILogger<MockEventBus> logger)
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

