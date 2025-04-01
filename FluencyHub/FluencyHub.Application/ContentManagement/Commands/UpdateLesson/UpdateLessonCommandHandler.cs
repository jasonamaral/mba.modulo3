using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.ContentManagement;
using MediatR;

namespace FluencyHub.Application.ContentManagement.Commands.UpdateLesson;

public class UpdateLessonCommandHandler : IRequestHandler<UpdateLessonCommand>
{
    private readonly FluencyHub.Application.Common.Interfaces.ICourseRepository _courseRepository;
    
    public UpdateLessonCommandHandler(FluencyHub.Application.Common.Interfaces.ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }
    
    public async Task Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(request.CourseId);
        
        if (course == null)
        {
            throw new NotFoundException(nameof(Course), request.CourseId);
        }
        
        var lesson = course.Lessons.FirstOrDefault(l => l.Id == request.LessonId);
        
        if (lesson == null)
        {
            throw new NotFoundException(nameof(Lesson), request.LessonId);
        }
        
        course.UpdateLesson(request.LessonId, request.Title, request.Content, request.MaterialUrl);
        
        await _courseRepository.SaveChangesAsync(cancellationToken);
    }
} 