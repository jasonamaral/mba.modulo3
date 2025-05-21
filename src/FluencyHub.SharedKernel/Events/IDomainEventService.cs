namespace FluencyHub.SharedKernel.Events;

/// <summary>
/// Interface para serviço de publicação de eventos de domínio
/// </summary>
public interface IDomainEventService
{
    /// <summary>
    /// Publicar todos os eventos de domínio de uma entidade
    /// </summary>
    /// <param name="events">Lista de eventos de domínio</param>
    Task PublishEventsAsync(IEnumerable<IDomainEvent> events);
    
    /// <summary>
    /// Publicar um evento de domínio
    /// </summary>
    /// <param name="event">O evento de domínio</param>
    Task PublishEventAsync(IDomainEvent @event);
    
    /// <summary>
    /// Publicar um evento de domínio (compatibilidade)
    /// </summary>
    /// <param name="event">O evento de domínio</param>
    Task PublishAsync(object @event);
} 