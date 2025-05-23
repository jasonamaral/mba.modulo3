using MediatR;

namespace FluencyHub.ContentManagement.Application.Commands.DeleteCourse;

public record DeleteCourseCommand : IRequest<bool>
{
    public required Guid Id { get; init; }
} 