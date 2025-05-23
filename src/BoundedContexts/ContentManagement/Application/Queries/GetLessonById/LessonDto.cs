namespace FluencyHub.ContentManagement.Application.Queries.GetLessonById;

public class LessonDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Order { get; set; }
    public int DurationInMinutes { get; set; }
    public string? VideoUrl { get; set; }
} 