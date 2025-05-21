using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.SharedKernel.Events;

/// <summary>
/// Implementação do serviço de publicação de eventos de domínio
/// </summary>
public class DomainEventService : IDomainEventService
{
    private readonly IPublisher _publisher;
    private readonly ILogger<DomainEventService> _logger;

    public DomainEventService(IPublisher publisher, ILogger<DomainEventService> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    /// <summary>
    /// Publicar todos os eventos de domínio de uma entidade
    /// </summary>
    /// <param name="events">Lista de eventos de domínio</param>
    public async Task PublishEventsAsync(IEnumerable<IDomainEvent> events)
    {
        foreach (var @event in events)
        {
            await PublishEventAsync(@event);
        }
    }

    /// <summary>
    /// Publicar um evento de domínio
    /// </summary>
    /// <param name="event">O evento de domínio</param>
    public async Task PublishEventAsync(IDomainEvent @event)
    {
        _logger.LogInformation("Publishing domain event: {EventName}", @event.GetType().Name);
        await _publisher.Publish(@event);
    }
    
    /// <summary>
    /// Publicar um evento de domínio (compatibilidade)
    /// </summary>
    /// <param name="event">O evento de domínio</param>
    public async Task PublishAsync(object @event)
    {
        if (@event is IDomainEvent domainEvent)
        {
            await PublishEventAsync(domainEvent);
        }
        else
        {
            _logger.LogWarning("Attempted to publish a non-domain event: {EventName}", @event.GetType().Name);
            await _publisher.Publish(@event);
        }
    }
} 