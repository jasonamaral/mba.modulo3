using MediatR;

namespace FluencyHub.ContentManagement.Application.Commands.AddLesson;

public record AddLessonCommand : IRequest<Guid>
{
    public required Guid CourseId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Content { get; init; }
    public int Order { get; init; }
    public int DurationMinutes { get; init; }
    public string? VideoUrl { get; init; }
} 