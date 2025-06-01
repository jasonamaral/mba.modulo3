using FluencyHub.SharedKernel.Events;

namespace FluencyHub.ContentManagement.Domain.Events;

public class CourseArchivedDomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid CourseId { get; }
    public string Name { get; }

    public CourseArchivedDomainEvent(Course course)
    {
        CourseId = course.Id;
        Name = course.Name;
    }
} 