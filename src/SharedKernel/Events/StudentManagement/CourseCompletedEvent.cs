namespace FluencyHub.SharedKernel.Events.StudentManagement;

/// <summary>
/// Evento disparado quando um aluno completa um curso
/// </summary>
public class CourseCompletedEvent : DomainEventBase
{
    public Guid StudentId { get; }
    public Guid CourseId { get; }
    public DateTime CompletionDate { get; }
    public int? FinalScore { get; }

    public CourseCompletedEvent(Guid studentId, Guid courseId, DateTime completionDate, int? finalScore = null)
    {
        StudentId = studentId;
        CourseId = courseId;
        CompletionDate = completionDate;
        FinalScore = finalScore;
    }
} 