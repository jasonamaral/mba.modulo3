using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.SharedKernel.Events;

/// <summary>
/// Implementação do serviço de publicação de eventos de domínio
/// </summary>
public class DomainEventService : IDomainEventService
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventService> _logger;

    public DomainEventService(IMediator mediator, ILogger<DomainEventService> logger)
    {
        _mediator = mediator;
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
        await _mediator.Publish(@event);
    }
    
    /// <summary>
    /// Publicar um evento de domínio (compatibilidade)
    /// </summary>
    /// <param name="event">O evento de domínio</param>
    public async Task PublishAsync(INotification domainEvent)
    {
        _logger.LogInformation("Publishing domain event. Event - {event}", domainEvent.GetType().Name);
        await _mediator.Publish(domainEvent);
    }
} 