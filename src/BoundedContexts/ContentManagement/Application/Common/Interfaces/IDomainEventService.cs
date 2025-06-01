namespace FluencyHub.ContentManagement.Application.Common.Interfaces;

public interface IDomainEventService
{
    Task PublishAsync(object domainEvent);
} 