using FluencyHub.SharedKernel.Events.StudentManagement;
using FluencyHub.ContentManagement.Application.Common.Interfaces;
using MediatR;

namespace FluencyHub.ContentManagement.Application.Handlers;

public class LessonCompletedEventHandler : INotificationHandler<LessonCompletedEvent>
{
    private readonly ICourseRepository _courseRepository;
    
    public LessonCompletedEventHandler(
        ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }
    
    public async Task Handle(LessonCompletedEvent notification, CancellationToken cancellationToken)
    {
        var lesson = await _courseRepository.GetLessonByIdAsync(notification.LessonId);
        if (lesson == null)
        {
            return;
        }
        
        lesson.IncrementCompletionCount();
        
        await _courseRepository.SaveChangesAsync();
    }
} 