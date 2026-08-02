using ECommerceAuction.UserService.Application;
using ECommerceAuction.UserService.Persistence.Context;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ECommerceAuction.UserService.Infrastructure.Messaging;

public static class MassTransitConfiguration
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<RabbitMqOptions>()
            .Bind(
                configuration.GetSection(
                    RabbitMqOptions.SectionName))
            .Validate(
                options =>
                    TryValidateConnectionString(
                        options.ConnectionString),
                "RabbitMQ connection string must be a valid " +
                "amqp:// or amqps:// URI.")
            .ValidateOnStart();

        services.AddMassTransit(configurator =>
        {
            configurator.SetKebabCaseEndpointNameFormatter();
            configurator.AddConsumers(typeof(ApplicationAssemblyMarker).Assembly);
            configurator.AddConfigureEndpointsCallback((context, _, endpoint) =>
            {
                // Serialize each queue so two deliveries carrying the same domain
                // MessageId cannot both pass the ledger idempotency check before
                // either transaction commits.
                endpoint.ConcurrentMessageLimit = 1;
                endpoint.UseMessageRetry(retry =>
                    retry.Interval(3, TimeSpan.FromSeconds(5)));
                endpoint.UseEntityFrameworkOutbox<ApplicationDbContext>(context);
            });

            configurator
                .AddEntityFrameworkOutbox<ApplicationDbContext>(
                    outbox =>
                    {
                        outbox.UseSqlServer();
                        outbox.UseBusOutbox();

                        outbox.QueryDelay =
                            TimeSpan.FromSeconds(1);
                    });

            configurator.UsingRabbitMq(
                (context, rabbitMq) =>
                {
                    var options = context
                        .GetRequiredService<
                            IOptions<RabbitMqOptions>>()
                        .Value;

                    ConfigureRabbitMqHost(
                        rabbitMq,
                        options.ConnectionString);

                    rabbitMq.ConfigureEndpoints(context);
                });
        });

        return services;
    }

    private static void ConfigureRabbitMqHost(
        IRabbitMqBusFactoryConfigurator rabbitMq,
        string connectionString)
    {
        var connectionUri = new Uri(
            connectionString,
            UriKind.Absolute);

        var hostUri = RemoveCredentials(
            connectionUri);

        rabbitMq.Host(
            hostUri,
            host =>
            {
                var credentials =
                    ParseCredentials(connectionUri);

                if (credentials is not null)
                {
                    host.Username(credentials.Value.Username);
                    host.Password(credentials.Value.Password);
                }

                if (connectionUri.Scheme.Equals(
                        "amqps",
                        StringComparison.OrdinalIgnoreCase))
                {
                    host.UseSsl();
                }
            });
    }

    private static Uri RemoveCredentials(Uri connectionUri)
    {
        var builder = new UriBuilder(connectionUri)
        {
            UserName = string.Empty,
            Password = string.Empty
        };

        return builder.Uri;
    }

    private static (
        string Username,
        string Password)?
        ParseCredentials(Uri connectionUri)
    {
        if (string.IsNullOrWhiteSpace(
                connectionUri.UserInfo))
        {
            return null;
        }

        var parts = connectionUri.UserInfo.Split(
            ':',
            2,
            StringSplitOptions.None);

        var username = Uri.UnescapeDataString(
            parts[0]);

        var password = parts.Length == 2
            ? Uri.UnescapeDataString(parts[1])
            : string.Empty;

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException(
                "RabbitMQ username is missing.");
        }

        return (username, password);
    }

    private static bool TryValidateConnectionString(
        string? connectionString)
    {
        if (!Uri.TryCreate(
                connectionString,
                UriKind.Absolute,
                out var uri))
        {
            return false;
        }

        return uri.Scheme.Equals(
                   "amqp",
                   StringComparison.OrdinalIgnoreCase) ||
               uri.Scheme.Equals(
                   "amqps",
                   StringComparison.OrdinalIgnoreCase);
    }
}
