using MediatR;
using FluencyHub.ContentManagement.Application.Common.Interfaces;

namespace FluencyHub.ContentManagement.Application.Queries.GetLessonById;

public class GetLessonByIdQueryHandler : IRequestHandler<GetLessonByIdQuery, LessonDto?>
{
    private readonly ILessonRepository _lessonRepository;

    public GetLessonByIdQueryHandler(ILessonRepository lessonRepository)
    {
        _lessonRepository = lessonRepository;
    }

    public async Task<LessonDto?> Handle(GetLessonByIdQuery request, CancellationToken cancellationToken)
    {
        var lesson = await _lessonRepository.GetByIdAsync(request.LessonId, cancellationToken);
        
        if (lesson == null)
            return null;

        return new LessonDto
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Description = lesson.Description ?? string.Empty,
            Content = lesson.Content,
            Order = lesson.Order,
            DurationInMinutes = lesson.DurationMinutes,
            VideoUrl = lesson.MaterialUrl
        };
    }
} 