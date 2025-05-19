using FluencyHub.ContentManagement.Domain;
using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.Application.ContentManagement.Commands.AddLesson;

public class AddLessonCommandHandler : IRequestHandler<AddLessonCommand, Guid>
{
    private readonly Application.Common.Interfaces.ICourseRepository _courseRepository;
    private readonly ILogger<AddLessonCommandHandler> _logger;

    public AddLessonCommandHandler(
        Application.Common.Interfaces.ICourseRepository courseRepository,
        ILogger<AddLessonCommandHandler> logger)
    {
        _courseRepository = courseRepository;
        _logger = logger;
    }

    public async Task<Guid> Handle(AddLessonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Adding lesson to course {CourseId}: {LessonTitle}", request.CourseId, request.Title);

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                _logger.LogWarning("Course not found: {CourseId}", request.CourseId);
                throw new NotFoundException(nameof(Course), request.CourseId);
            }

            var lesson = course.AddLesson(
                request.Title,
                request.Content,
                request.MaterialUrl);

            await _courseRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Lesson added successfully to course {CourseId}, lesson ID: {LessonId}", request.CourseId, lesson.Id);

            return lesson.Id;
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            _logger.LogError(ex, "Error adding lesson to course {CourseId}", request.CourseId);
            throw;
        }
    }
}