namespace FluencyHub.SharedKernel.Events.ContentManagement;

/// <summary>
/// Evento disparado quando um novo curso é criado
/// </summary>
public class CourseCreatedEvent : DomainEventBase
{
    public Guid CourseId { get; }
    public string Title { get; }
    public string Description { get; }
    public int TotalLessons { get; }
    public decimal Price { get; }

    public CourseCreatedEvent(Guid courseId, string title, string description, int totalLessons, decimal price)
    {
        CourseId = courseId;
        Title = title;
        Description = description;
        TotalLessons = totalLessons;
        Price = price;
    }
} 