using FluencyHub.PaymentProcessing.Domain.Common;

namespace FluencyHub.PaymentProcessing.Application.Common.Interfaces;

public interface IDomainEventService
{
    Task PublishAsync(object domainEvent);
} 