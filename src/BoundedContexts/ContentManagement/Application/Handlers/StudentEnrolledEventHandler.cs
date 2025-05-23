using FluencyHub.SharedKernel.Events.StudentManagement;
using FluencyHub.ContentManagement.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.ContentManagement.Application.Handlers;

public class StudentEnrolledEventHandler : INotificationHandler<StudentEnrolledEvent>
{
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<StudentEnrolledEventHandler> _logger;
    
    public StudentEnrolledEventHandler(
        ICourseRepository courseRepository,
        ILogger<StudentEnrolledEventHandler> logger)
    {
        _courseRepository = courseRepository;
        _logger = logger;
    }
    
    public async Task Handle(StudentEnrolledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling StudentEnrolledEvent for StudentId: {StudentId}, CourseId: {CourseId}", 
            notification.StudentId, notification.CourseId);
        
        var course = await _courseRepository.GetByIdWithLessonsAsync(notification.CourseId, cancellationToken);
        if (course == null)
        {
            _logger.LogWarning("Course not found for id: {CourseId}", notification.CourseId);
            return;
        }
        
        course.IncrementEnrollmentCount();
        _logger.LogInformation("Enrollment count incremented for course: {CourseId}", course.Id);
        
        await _courseRepository.UpdateAsync(course);
    }
} 