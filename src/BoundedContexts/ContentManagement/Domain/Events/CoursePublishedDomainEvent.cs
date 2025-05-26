using FluencyHub.SharedKernel.Events;

namespace FluencyHub.ContentManagement.Domain.Events;

public class CoursePublishedDomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid CourseId { get; }
    public string Name { get; }
    public DateTime PublishedAt { get; }

    public CoursePublishedDomainEvent(Course course)
    {
        CourseId = course.Id;
        Name = course.Name;
        PublishedAt = course.PublishedAt ?? DateTime.UtcNow;
    }
} 