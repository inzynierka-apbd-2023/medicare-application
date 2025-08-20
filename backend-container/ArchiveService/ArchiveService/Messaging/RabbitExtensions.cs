using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace ArchiveService.Messaging;

public static class RabbitExtensions
{
    public static IServiceCollection AddRabbit(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory
        {
            HostName = config["RABBITMQ:HOST"] ?? config["RabbitMQ:Host"] ?? "rabbitmq",
            UserName = config["RABBITMQ:USERNAME"] ?? config["RabbitMQ:Username"] ?? "guest",
            Password = config["RABBITMQ:PASSWORD"] ?? config["RabbitMQ:Password"] ?? "guest",
            DispatchConsumersAsync = true
        });
        services.AddSingleton<IConnection>(sp => sp.GetRequiredService<IConnectionFactory>().CreateConnection());
        return services;
    }
}
