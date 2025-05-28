using FluencyHub.ContentManagement.Domain;
using FluencyHub.ContentManagement.Application.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using ICourseRepository = FluencyHub.ContentManagement.Application.Common.Interfaces.ICourseRepository;

namespace FluencyHub.ContentManagement.Application.Commands.UpdateCourse;

public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, bool>
{
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<UpdateCourseCommandHandler> _logger;

    public UpdateCourseCommandHandler(
        ICourseRepository courseRepository,
        ILogger<UpdateCourseCommandHandler> logger)
    {
        _courseRepository = courseRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {


        var course = await _courseRepository.GetByIdAsync(request.Id);
        if (course == null)
        {
            _logger.LogWarning("Course with ID {CourseId} not found", request.Id);
            throw new NotFoundException($"Course with ID {request.Id} not found");
        }

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
} 