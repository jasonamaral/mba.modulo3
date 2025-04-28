using MediatR;

namespace FluencyHub.Application.ContentManagement.Queries.GetLessonsByCourse;

public record GetLessonsByCourseQuery(Guid CourseId) : IRequest<IEnumerable<LessonDto>>;

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