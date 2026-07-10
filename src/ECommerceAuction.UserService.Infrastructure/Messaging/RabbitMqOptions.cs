using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Infrastructure.Messaging
{
    public sealed class RabbitMqOptions
    {
        public const string SectionName = "RabbitMQ";
        public string Host { get; init; } = "rabbitmq";
        public string Username { get; init; } = "guest";
        public string Password { get; init; } = "guest";
        public string VHost { get; init; } = "/";
        public bool UseSsl { get; init; } = false;
        public int? Port {  get; init; } = null;
    }
}
