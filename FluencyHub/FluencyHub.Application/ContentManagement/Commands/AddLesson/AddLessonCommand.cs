using MediatR;

namespace FluencyHub.Application.ContentManagement.Commands.AddLesson;

public record AddLessonCommand : IRequest<Guid>
{
    public Guid CourseId { get; init; }
    public string Title { get; init; }
    public string Content { get; init; }
    public string? MaterialUrl { get; init; }
} 