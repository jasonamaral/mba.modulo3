using FluencyHub.Domain.Common;

namespace FluencyHub.StudentManagement.Application.Common.Interfaces;

public interface IDomainEventService
{
    Task PublishAsync(DomainEvent domainEvent);
} 