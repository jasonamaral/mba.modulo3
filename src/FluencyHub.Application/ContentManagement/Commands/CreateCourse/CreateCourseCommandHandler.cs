using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.ContentManagement;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.Application.ContentManagement.Commands.CreateCourse;

public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Guid>
{
    private readonly Application.Common.Interfaces.ICourseRepository _courseRepository;
    private readonly ILogger<CreateCourseCommandHandler> _logger;
    
    public CreateCourseCommandHandler(
        Application.Common.Interfaces.ICourseRepository courseRepository,
        ILogger<CreateCourseCommandHandler> logger)
    {
        _courseRepository = courseRepository;
        _logger = logger;
    }
    
    public async Task<Guid> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating new course: {CourseName}", request.Name);
            
            // Create the course content value object
            var courseContent = new CourseContent(
                request.Syllabus,
                request.LearningObjectives,
                request.PreRequisites,
                request.TargetAudience,
                request.Language,
                request.Level);
                
            // Create the course entity
            var course = new Course(
                request.Name,
                request.Description,
                courseContent,
                request.Price);
                
            // Save to repository
            await _courseRepository.AddAsync(course);
            await _courseRepository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Course created successfully with ID: {CourseId}", course.Id);
            
            return course.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course {CourseName}", request.Name);
            throw;
        }
    }
} 