namespace FluencyHub.PaymentProcessing.Application.Common.Interfaces;

public interface IDomainEventService
{
    Task PublishAsync(object domainEvent);
} 