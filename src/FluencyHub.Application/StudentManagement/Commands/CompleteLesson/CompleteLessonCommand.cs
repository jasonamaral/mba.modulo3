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
    public Guid EnrollmentId { get; set; }
    public Guid LessonId { get; set; }
    public bool Completed { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool CourseCompleted { get; set; }
    public bool IsCompleted { get; set; }
    public bool AllLessonsCompleted { get; set; }
} 