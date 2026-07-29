namespace ECommerceAuction.UserService.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";
    public string ConnectionString { get; init; } = string.Empty;
}