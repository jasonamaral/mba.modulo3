using FluencyHub.Domain.Common;

namespace FluencyHub.ContentManagement.Application.Common.Interfaces;

public interface IDomainEventService
{
    Task PublishAsync(DomainEvent domainEvent);
} 