using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.CompleteCourseForStudent;

public record CompleteCourseForStudentCommand : IRequest<CompleteCourseForStudentResult>
{
    public Guid StudentId { get; init; }
    public Guid CourseId { get; init; }

    public CompleteCourseForStudentCommand(Guid studentId, Guid courseId)
    {
        StudentId = studentId;
        CourseId = courseId;
    }
}

public class CompleteCourseForStudentResult
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
} 