using FluencyHub.Domain.Common;

namespace FluencyHub.PaymentProcessing.Application.Common.Interfaces;

public interface IDomainEventService
{
    Task PublishAsync(DomainEvent domainEvent);
} 