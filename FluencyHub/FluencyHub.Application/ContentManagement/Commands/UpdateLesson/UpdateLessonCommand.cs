using MediatR;

namespace FluencyHub.Application.ContentManagement.Commands.UpdateLesson;

public record UpdateLessonCommand : IRequest
{
    public Guid CourseId { get; init; }
    public Guid LessonId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? MaterialUrl { get; init; }
} 