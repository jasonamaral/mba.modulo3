using FluencyHub.StudentManagement.Domain.Common;

namespace FluencyHub.StudentManagement.Application.Common.Interfaces;

public interface IDomainEventService
{
    Task PublishAsync(object domainEvent);
} 