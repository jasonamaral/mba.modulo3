namespace FluencyHub.StudentManagement.Application.Commands.CompleteLessonForStudent;

public class CompleteLessonForStudentResult
{
    public Guid StudentId { get; set; }
    public Guid LessonId { get; set; }
    public bool IsSuccessful { get; set; }
    public int? Score { get; set; }
    public int? CourseProgress { get; set; }
    public bool CourseCompleted { get; set; }
} 