using FluencyHub.ContentManagement.Domain;
using FluencyHub.ContentManagement.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

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
        _logger.LogInformation("Creating new course with name: {Name}", request.Name);

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
            request.Price)
        {
            Name = request.Name,
            Description = request.Description,
            Content = courseContent
        };

        await _courseRepository.AddAsync(course);
        await _courseRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Course created successfully with ID: {CourseId}", course.Id);

        return course.Id;
    }
} 