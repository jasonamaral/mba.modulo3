using FluencyHub.SharedKernel.Events;

namespace FluencyHub.SharedKernel.DomainModels;

/// <summary>
/// Classe base para entidades de domínio com suporte a eventos
/// </summary>
public abstract class EntityBase : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    /// <summary>
    /// Identificador único da entidade
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Lista de eventos de domínio não publicados
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adicionar um novo evento de domínio
    /// </summary>
    /// <param name="domainEvent">O evento de domínio</param>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Remover um evento de domínio
    /// </summary>
    /// <param name="domainEvent">O evento de domínio</param>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Limpar todos os eventos de domínio
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
} 