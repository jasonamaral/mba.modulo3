namespace FluencyHub.StudentManagement.Domain;

public class CompletedLesson
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LessonId { get; set; }
    public Guid CourseProgressId { get; set; }
    public DateTime CompletedAt { get; set; }

    // Navigation property - não required para evitar problemas de referência circular
    public CourseProgress? CourseProgress { get; set; }
} 