namespace FluencyHub.Application.ContentManagement.Queries.GetCourseById;

public record CourseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public CourseContentDto Content { get; init; } = null!;
    public decimal Price { get; init; }
    public IEnumerable<LessonDto> Lessons { get; init; } = new List<LessonDto>();
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CourseContentDto
{
    public string Syllabus { get; init; } = string.Empty;
    public string LearningObjectives { get; init; } = string.Empty;
    public string PreRequisites { get; init; } = string.Empty;
    public string TargetAudience { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public string Level { get; init; } = string.Empty;
}

public record LessonDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? MaterialUrl { get; init; }
    public int Order { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
} 