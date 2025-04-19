using MediatR;

namespace FluencyHub.Application.StudentManagement.Queries.GetStudentProgress;

public record GetStudentProgressQuery : IRequest<Dictionary<Guid, Dictionary<Guid, bool>>>
{
    public Guid StudentId { get; init; }

    public GetStudentProgressQuery(Guid studentId)
    {
        StudentId = studentId;
    }
}

public class StudentProgressViewModel
{
    public Dictionary<Guid, CourseProgressDto> Progress { get; set; } = [];
}

public class CourseProgressDto
{
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime LastUpdated { get; set; }
} 