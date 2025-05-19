using FluencyHub.PaymentProcessing.Domain.Common;

namespace FluencyHub.Application.Common.Interfaces;

public interface IDomainEventService
{
    Task PublishAsync(object domainEvent);
} 