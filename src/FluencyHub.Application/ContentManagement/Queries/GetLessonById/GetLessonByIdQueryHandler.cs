using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.ContentManagement;
using MediatR;

namespace FluencyHub.Application.ContentManagement.Queries.GetLessonById;

public class GetLessonByIdQueryHandler : IRequestHandler<GetLessonByIdQuery, LessonDto>
{
    private readonly FluencyHub.Application.Common.Interfaces.ICourseRepository _courseRepository;
    
    public GetLessonByIdQueryHandler(FluencyHub.Application.Common.Interfaces.ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }
    
    public async Task<LessonDto> Handle(GetLessonByIdQuery request, CancellationToken cancellationToken)
    {
        var lesson = await _courseRepository.GetLessonByIdAsync(request.LessonId);
        
        if (lesson == null)
        {
            throw new NotFoundException(nameof(Lesson), request.LessonId);
        }
        
        return new LessonDto
        {
            Id = lesson.Id,
            Title = lesson.Title,
            Content = lesson.Content,
            MaterialUrl = lesson.MaterialUrl,
            Order = lesson.Order,
            CourseId = lesson.CourseId,
            IsActive = true,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt
        };
    }
} 