namespace FluencyHub.SharedKernel.Events.ContentManagement;

/// <summary>
/// Evento disparado quando um curso é atualizado
/// </summary>
public class CourseUpdatedEvent : DomainEventBase
{
    public Guid CourseId { get; }
    public string Title { get; }
    public string Description { get; }
    public int TotalLessons { get; }
    public decimal Price { get; }
    public bool IsActive { get; }

    public CourseUpdatedEvent(Guid courseId, string title, string description, 
        int totalLessons, decimal price, bool isActive)
    {
        CourseId = courseId;
        Title = title;
        Description = description;
        TotalLessons = totalLessons;
        Price = price;
        IsActive = isActive;
    }
} 