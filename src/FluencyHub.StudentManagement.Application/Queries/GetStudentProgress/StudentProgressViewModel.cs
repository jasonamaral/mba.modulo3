namespace FluencyHub.StudentManagement.Application.Queries.GetStudentProgress;

public class StudentProgressViewModel
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int ProgressPercentage { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletionDate { get; set; }
    public List<LessonProgressDto> LessonProgress { get; set; } = new List<LessonProgressDto>();
}

public class LessonProgressDto
{
    public Guid LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletionDate { get; set; }
    public int? Score { get; set; }
} 