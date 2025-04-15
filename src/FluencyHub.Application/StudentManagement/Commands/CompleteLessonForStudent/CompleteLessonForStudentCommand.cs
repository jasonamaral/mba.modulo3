using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.CompleteLessonForStudent;

public record CompleteLessonForStudentCommand : IRequest<CompleteLessonForStudentResult>
{
    public Guid StudentId { get; init; }
    public Guid CourseId { get; init; }
    public Guid LessonId { get; init; }

    public CompleteLessonForStudentCommand(Guid studentId, Guid courseId, Guid lessonId)
    {
        StudentId = studentId;
        CourseId = courseId;
        LessonId = lessonId;
    }
}

public class CompleteLessonForStudentResult
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public Guid LessonId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
} 