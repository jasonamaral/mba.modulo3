using FluencyHub.SharedKernel.Events.StudentManagement;
using FluencyHub.ContentManagement.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.ContentManagement.Application.Handlers;

public class LessonCompletedEventHandler : INotificationHandler<LessonCompletedEvent>
{
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<LessonCompletedEventHandler> _logger;
    
    public LessonCompletedEventHandler(
        ICourseRepository courseRepository,
        ILogger<LessonCompletedEventHandler> logger)
    {
        _courseRepository = courseRepository;
        _logger = logger;
    }
    
    public async Task Handle(LessonCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling LessonCompletedEvent for LessonId: {LessonId}, StudentId: {StudentId}", 
            notification.LessonId, notification.StudentId);
        
        var lesson = await _courseRepository.GetLessonByIdAsync(notification.LessonId);
        if (lesson == null)
        {
            _logger.LogWarning("Lesson not found for id: {LessonId}", notification.LessonId);
            return;
        }
        
        lesson.IncrementCompletionCount();
        _logger.LogInformation("Completion count incremented for lesson: {LessonId}", lesson.Id);
        
        await _courseRepository.SaveChangesAsync();
    }
} 