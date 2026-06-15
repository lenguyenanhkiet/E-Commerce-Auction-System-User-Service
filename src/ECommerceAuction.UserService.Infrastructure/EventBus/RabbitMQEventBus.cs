using System.Text.Json;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace ECommerceAuction.UserService.Infrastructure.EventBus;

/// <summary>
/// Temporary event bus implementation used until the real RabbitMQ producer/consumer is completed.
/// </summary>
public sealed class RabbitMQEventBus : IEventBus
{
    private readonly ILogger<RabbitMQEventBus> _logger;

    public RabbitMQEventBus(ILogger<RabbitMQEventBus> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Publishes an event by logging it. Kiệt's Email Service can later replace this with real RabbitMQ delivery.
    /// </summary>
    public Task PublishAsync<TEvent>(TEvent eventMessage, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        // Kiệt - Email integration: while email is not ready, log payload so the OTP can be read during manual testing.
        _logger.LogInformation(
            "Mock event published: {EventType} {Payload}",
            typeof(TEvent).Name,
            JsonSerializer.Serialize(eventMessage));

        return Task.CompletedTask;
    }
}
