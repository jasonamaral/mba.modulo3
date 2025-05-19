using AutoMapper;
using FluencyHub.ContentManagement.Domain;
using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using MediatR;

namespace FluencyHub.Application.ContentManagement.Queries.GetLessonsByCourse;

public class GetLessonsByCourseQueryHandler : IRequestHandler<GetLessonsByCourseQuery, IEnumerable<LessonDto>>
{
    private readonly FluencyHub.Application.Common.Interfaces.ICourseRepository _courseRepository;
    
    public GetLessonsByCourseQueryHandler(FluencyHub.Application.Common.Interfaces.ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }
    
    public async Task<IEnumerable<LessonDto>> Handle(GetLessonsByCourseQuery request, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdWithLessonsAsync(request.CourseId);
        
        if (course == null)
        {
            throw new NotFoundException(nameof(Course), request.CourseId);
        }
        
        return course.Lessons.Select(lesson => new LessonDto
        {
            Id = lesson.Id,
            Title = lesson.Title,
            Content = lesson.Content,
            MaterialUrl = lesson.MaterialUrl,
            Order = lesson.Order,
            IsActive = true, // Todos os lessons disponíveis são ativos por padrão
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt
        });
    }
} 