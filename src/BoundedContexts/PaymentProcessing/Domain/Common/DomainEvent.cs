using FluencyHub.SharedKernel.Events;

namespace FluencyHub.PaymentProcessing.Domain.Common;

public abstract class DomainEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    
    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
} 