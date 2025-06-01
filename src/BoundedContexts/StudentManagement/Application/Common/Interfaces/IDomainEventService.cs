namespace FluencyHub.StudentManagement.Application.Common.Interfaces;

public interface IDomainEventService
{
    Task PublishAsync(object domainEvent);
} 