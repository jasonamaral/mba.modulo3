using FluencyHub.Domain.Common;

namespace FluencyHub.Application.Common.Interfaces;

public interface IDomainEventService
{
    Task PublishAsync(DomainEvent domainEvent);
} 