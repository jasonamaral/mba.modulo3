using FluencyHub.SharedKernel.Events;
using FluencyHub.SharedKernel.Enums;

namespace FluencyHub.ContentManagement.Domain.Events;

public class CourseCreatedDomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid CourseId { get; }
    public string Name { get; }
    public string Description { get; }
    public decimal Price { get; }
    public bool IsActive { get; }
    public CourseStatus Status { get; }
    public CourseContent Content { get; }

    public CourseCreatedDomainEvent(Course course)
    {
        CourseId = course.Id;
        Name = course.Name;
        Description = course.Description;
        Price = course.Price;
        IsActive = course.IsActive;
        Status = course.Status;
        Content = course.Content;
    }
} 