namespace FluencyHub.SharedKernel.Events.StudentManagement;

/// <summary>
/// Evento disparado quando um aluno completa uma lição
/// </summary>
public class LessonCompletedEvent : DomainEventBase
{
    public Guid EnrollmentId { get; }
    public Guid LessonId { get; }
    public Guid StudentId { get; }
    public Guid CourseId { get; }
    public DateTime CompletionDate { get; }

    public LessonCompletedEvent(Guid enrollmentId, Guid lessonId, Guid studentId, Guid courseId, DateTime completionDate)
    {
        EnrollmentId = enrollmentId;
        LessonId = lessonId;
        StudentId = studentId;
        CourseId = courseId;
        CompletionDate = completionDate;
    }
} 