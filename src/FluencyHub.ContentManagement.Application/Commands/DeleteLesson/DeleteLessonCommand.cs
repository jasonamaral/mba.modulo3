using MediatR;

namespace FluencyHub.ContentManagement.Application.Commands.DeleteLesson;

public record DeleteLessonCommand : IRequest<bool>
{
    public required Guid Id { get; init; }
} 