namespace FluencyHub.ContentManagement.Domain.Common;

public abstract class DomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    
    protected DomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
} 