using MediatR;

namespace FluencyHub.Application.StudentManagement.Queries.GetStudentDebugProgress;

public record GetStudentDebugProgressQuery : IRequest<StudentDebugProgressViewModel>
{
    public Guid StudentId { get; init; }

    public GetStudentDebugProgressQuery(Guid studentId)
    {
        StudentId = studentId;
    }
}

public class StudentDebugProgressViewModel
{
    public List<CourseProgressDebugInfo> DebugInfo { get; set; } = new List<CourseProgressDebugInfo>();
}

public class CourseProgressDebugInfo
{
    public Guid CourseProgressId { get; set; }
    public Guid CourseId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? CompletedLessonsRaw { get; set; }
} 