namespace FluencyHub.SharedKernel.Events.StudentManagement;

/// <summary>
/// Evento disparado quando um aluno completa uma lição
/// </summary>
public class LessonCompletedEvent : DomainEventBase
{
    public Guid StudentId { get; }
    public Guid CourseId { get; }
    public Guid LessonId { get; }
    public DateTime CompletionDate { get; }

    public LessonCompletedEvent(Guid studentId, Guid courseId, Guid lessonId, DateTime completionDate)
    {
        StudentId = studentId;
        CourseId = courseId;
        LessonId = lessonId;
        CompletionDate = completionDate;
    }
} 