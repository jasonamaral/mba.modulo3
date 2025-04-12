using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.ContentManagement;
using MediatR;

namespace FluencyHub.Application.ContentManagement.Queries.GetCourseById;

public class GetCourseByIdQueryHandler : IRequestHandler<GetCourseByIdQuery, CourseDto>
{
    private readonly Application.Common.Interfaces.ICourseRepository _courseRepository;
    
    public GetCourseByIdQueryHandler(Application.Common.Interfaces.ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }
    
    public async Task<CourseDto> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(request.Id);
        
        if (course == null)
        {
            throw new NotFoundException(nameof(Course), request.Id);
        }
        
        return new CourseDto
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description,
            Content = new CourseContentDto
            {
                Syllabus = course.Content.Syllabus,
                LearningObjectives = course.Content.LearningObjectives,
                PreRequisites = course.Content.PreRequisites,
                TargetAudience = course.Content.TargetAudience,
                Language = course.Content.Language,
                Level = course.Content.Level
            },
            Lessons = course.Lessons.Select(l => new LessonDto
            {
                Id = l.Id,
                Title = l.Title,
                Content = l.Content,
                MaterialUrl = l.MaterialUrl,
                Order = l.Order
            }),
            IsActive = course.IsActive,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt
        };
    }
} 