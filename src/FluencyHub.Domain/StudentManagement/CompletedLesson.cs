namespace FluencyHub.Domain.StudentManagement;

public class CompletedLesson
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LessonId { get; set; }
    public Guid CourseProgressId { get; set; }
    public DateTime CompletedAt { get; set; }

    public required CourseProgress CourseProgress { get; set; }
}