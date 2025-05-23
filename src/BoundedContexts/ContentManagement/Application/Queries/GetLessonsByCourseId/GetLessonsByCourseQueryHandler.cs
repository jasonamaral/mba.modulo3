using MediatR;
using FluencyHub.ContentManagement.Application.Common.Models;
using FluencyHub.ContentManagement.Application.Common.Interfaces;

namespace FluencyHub.ContentManagement.Application.Queries.GetLessonsByCourseId;

public class GetLessonsByCourseQueryHandler : IRequestHandler<GetLessonsByCourseIdQuery, IEnumerable<LessonDto>>
{
    private readonly ILessonRepository _lessonRepository;

    public GetLessonsByCourseQueryHandler(ILessonRepository lessonRepository)
    {
        _lessonRepository = lessonRepository;
    }

    public async Task<IEnumerable<LessonDto>> Handle(GetLessonsByCourseIdQuery request, CancellationToken cancellationToken)
    {
        var lessons = await _lessonRepository.GetByCourseIdAsync(request.CourseId, cancellationToken);
        
        return lessons.Select(lesson => new LessonDto
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Description = lesson.Description ?? string.Empty,
            Content = lesson.Content,
            Order = lesson.Order,
            DurationMinutes = lesson.DurationMinutes,
            IsActive = true,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt
        });
    }
} 