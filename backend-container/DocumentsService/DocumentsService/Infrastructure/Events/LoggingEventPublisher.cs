using Microsoft.Extensions.Logging;

namespace DocumentsService.Infrastructure.Events;

public sealed class LoggingEventPublisher(ILogger<LoggingEventPublisher> logger) : IEventPublisher
{
    private readonly ILogger<LoggingEventPublisher> _logger = logger;
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
    {
        _logger.LogInformation("Event published: {EventType} {@Event}", typeof(TEvent).Name, @event);
        return Task.CompletedTask;
    }
}
