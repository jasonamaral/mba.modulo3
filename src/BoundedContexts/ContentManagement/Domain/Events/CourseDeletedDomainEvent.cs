using FluencyHub.SharedKernel.Events;

namespace FluencyHub.ContentManagement.Domain.Events;

public class CourseDeletedDomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid CourseId { get; }

    public CourseDeletedDomainEvent(Course course)
    {
        CourseId = course.Id;
    }
} 