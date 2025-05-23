namespace FluencyHub.SharedKernel.Events.ContentManagement;

/// <summary>
/// Evento disparado quando um curso é excluído
/// </summary>
public class CourseDeletedEvent : DomainEventBase
{
    public Guid CourseId { get; }

    public CourseDeletedEvent(Guid courseId)
    {
        CourseId = courseId;
    }
} 