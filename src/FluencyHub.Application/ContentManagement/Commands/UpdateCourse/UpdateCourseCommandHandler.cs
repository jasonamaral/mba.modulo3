using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Domain.ContentManagement;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.Application.ContentManagement.Commands.UpdateCourse;

public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, bool>
{
    private readonly Common.Interfaces.ICourseRepository _courseRepository;
    private readonly ILogger<UpdateCourseCommandHandler> _logger;

    public UpdateCourseCommandHandler(
        Common.Interfaces.ICourseRepository courseRepository,
        ILogger<UpdateCourseCommandHandler> logger)
    {
        _courseRepository = courseRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var course = await _courseRepository.GetByIdAsync(request.Id) ?? throw new NotFoundException($"Course with ID {request.Id} not found");

            var updatedContent = new CourseContent(
                request.Syllabus,
                request.LearningObjectives,
                request.PreRequisites,
                request.TargetAudience,
                request.Language,
                request.Level);

            course.UpdateDetails(
                request.Name,
                request.Description,
                updatedContent,
                request.Price);

            await _courseRepository.UpdateAsync(course);
            await _courseRepository.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course {CourseId}", request.Id);
            throw;
        }
    }
}