using MediatR;

namespace FluencyHub.Application.ContentManagement.Commands.DeleteLesson;

public record DeleteLessonCommand : IRequest
{
    public Guid CourseId { get; init; }
    public Guid LessonId { get; init; }

    public DeleteLessonCommand(Guid courseId, Guid lessonId)
    {
        CourseId = courseId;
        LessonId = lessonId;
    }
} 