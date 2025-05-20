using FluencyHub.ContentManagement.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using FluencyHub.ContentManagement.Application.Common.Interfaces;

namespace FluencyHub.ContentManagement.Application.Commands.CreateCourse;

public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Guid>
{
    private readonly FluencyHub.ContentManagement.Application.Common.Interfaces.ICourseRepository _courseRepository;
    private readonly ILogger<CreateCourseCommandHandler> _logger;

    public CreateCourseCommandHandler(
        FluencyHub.ContentManagement.Application.Common.Interfaces.ICourseRepository courseRepository,
        ILogger<CreateCourseCommandHandler> logger)
    {
        _courseRepository = courseRepository;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var courseContent = new CourseContent(
                request.Syllabus,
                request.LearningObjectives,
                request.PreRequisites,
                request.TargetAudience,
                request.Language,
                request.Level);

            var course = new Course(
                request.Name,
                request.Description,
                courseContent,
                request.Price);

            await _courseRepository.AddAsync(course);
            await _courseRepository.SaveChangesAsync(cancellationToken);

            return course.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course {CourseName}", request.Name);
            throw;
        }
    }
} 