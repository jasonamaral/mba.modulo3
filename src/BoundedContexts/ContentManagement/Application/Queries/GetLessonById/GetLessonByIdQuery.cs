using MediatR;

namespace FluencyHub.ContentManagement.Application.Queries.GetLessonById;

public record GetLessonByIdQuery : IRequest<LessonDto?>
{
    public required Guid LessonId { get; init; }
} 