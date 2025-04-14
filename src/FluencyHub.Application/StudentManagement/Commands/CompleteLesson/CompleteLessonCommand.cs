using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.CompleteLesson;

public record CompleteLessonCommand : IRequest<CompleteLessonResult>
{
    public Guid EnrollmentId { get; init; }
    public Guid LessonId { get; init; }
    public bool Completed { get; init; }
}

public class CompleteLessonResult
{
    public Guid EnrollmentId { get; init; }
    public Guid LessonId { get; init; }
    public bool Completed { get; init; }
    public string Message { get; init; }
    public bool CourseCompleted { get; init; }
} 