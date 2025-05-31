using FluencyHub.SharedKernel.Events.StudentManagement;
using FluencyHub.ContentManagement.Application.Common.Interfaces;
using MediatR;

namespace FluencyHub.ContentManagement.Application.Handlers;

public class StudentEnrolledEventHandler : INotificationHandler<StudentEnrolledEvent>
{
    private readonly ICourseRepository _courseRepository;
    
    public StudentEnrolledEventHandler(
        ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }
    
    public async Task Handle(StudentEnrolledEvent notification, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdWithLessonsAsync(notification.CourseId, cancellationToken);
        if (course == null)
        {
            return;
        }
        
        course.IncrementEnrollmentCount();
        
        await _courseRepository.UpdateAsync(course);
    }
} 