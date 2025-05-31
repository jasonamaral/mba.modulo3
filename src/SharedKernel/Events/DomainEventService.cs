using MediatR;

namespace FluencyHub.SharedKernel.Events;

/// <summary>
/// Implementação do serviço de publicação de eventos de domínio
/// </summary>
public class DomainEventService : IDomainEventService
{
    private readonly IMediator _mediator;

    public DomainEventService(IMediator mediator)
    {
        _mediator = mediator;
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
        await _mediator.Publish(@event);
    }
    
    /// <summary>
    /// Publicar um evento de domínio (compatibilidade)
    /// </summary>
    /// <param name="event">O evento de domínio</param>
    public async Task PublishAsync(INotification domainEvent)
    {
        await _mediator.Publish(domainEvent);
    }
} 