namespace FluencyHub.Application.ContentManagement.Queries.GetLessonById;

public class LessonDto
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public string? MaterialUrl { get; set; }
    public required int Order { get; set; }
    public required Guid CourseId { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 