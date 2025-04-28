using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.Application.ContentManagement.Commands.DeleteLesson;

public class DeleteLessonCommandHandler : IRequestHandler<DeleteLessonCommand>
{
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<DeleteLessonCommandHandler> _logger;

    public DeleteLessonCommandHandler(
        ICourseRepository courseRepository,
        ILogger<DeleteLessonCommandHandler> logger)
    {
        _courseRepository = courseRepository;
        _logger = logger;
    }

    public async Task Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(request.CourseId);
        
        if (course == null)
        {
            throw new NotFoundException("Course", request.CourseId);
        }

        var lesson = course.Lessons.FirstOrDefault(l => l.Id == request.LessonId);
        
        if (lesson == null)
        {
            throw new NotFoundException("Lesson", request.LessonId);
        }

        course.RemoveLesson(lesson.Id);
        
        await _courseRepository.UpdateAsync(course);
        await _courseRepository.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Lesson {LessonId} removed from course {CourseId}", request.LessonId, request.CourseId);
    }
} 