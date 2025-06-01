using MediatR;

namespace FluencyHub.SharedKernel.Events;

/// <summary>
/// Interface que representa um evento de domínio
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Identificador único do evento
    /// </summary>
    Guid EventId { get; }
    
    /// <summary>
    /// Timestamp de quando o evento ocorreu
    /// </summary>
    DateTime OccurredOn { get; }
} 