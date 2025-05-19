using FluencyHub.ContentManagement.Domain.Common;

namespace FluencyHub.ContentManagement.Application.Common.Interfaces;

public interface IDomainEventService
{
    Task PublishAsync(object domainEvent);
} 