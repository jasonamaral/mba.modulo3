using MediatR;

namespace FluencyHub.SharedKernel.Events.Integration;

public class CourseCompletedEvent : INotification
{
    public Guid CourseId { get; }
    public Guid StudentId { get; }
    public string CourseName { get; }
    public DateTime CompletedAt { get; }
    public decimal FinalGrade { get; }

    public CourseCompletedEvent(Guid courseId, Guid studentId, string courseName, DateTime completedAt, decimal finalGrade)
    {
        CourseId = courseId;
        StudentId = studentId;
        CourseName = courseName;
        CompletedAt = completedAt;
        FinalGrade = finalGrade;
    }
} 