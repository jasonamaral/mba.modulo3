using FluencyHub.SharedKernel.Events;

namespace FluencyHub.SharedKernel.DomainModels;

/// <summary>
/// Interface para entidades que podem gerar eventos de domínio
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Lista de eventos de domínio não publicados
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    
    /// <summary>
    /// Adicionar um novo evento de domínio
    /// </summary>
    /// <param name="domainEvent">O evento de domínio</param>
    void AddDomainEvent(IDomainEvent domainEvent);
    
    /// <summary>
    /// Remover um evento de domínio
    /// </summary>
    /// <param name="domainEvent">O evento de domínio</param>
    void RemoveDomainEvent(IDomainEvent domainEvent);
    
    /// <summary>
    /// Limpar todos os eventos de domínio
    /// </summary>
    void ClearDomainEvents();
} 