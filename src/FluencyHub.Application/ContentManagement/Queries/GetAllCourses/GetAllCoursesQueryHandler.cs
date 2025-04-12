using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.ContentManagement.Queries.GetCourseById;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FluencyHub.Application.ContentManagement.Queries.GetAllCourses;

public class GetAllCoursesQueryHandler : IRequestHandler<GetAllCoursesQuery, IEnumerable<CourseDto>>
{
    private readonly ICourseRepository _courseRepository;
    
    public GetAllCoursesQueryHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }
    
    public async Task<IEnumerable<CourseDto>> Handle(GetAllCoursesQuery request, CancellationToken cancellationToken)
    {
        var courses = await _courseRepository.GetAllAsync();
        
        return courses.Select(c => new CourseDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Content = new CourseContentDto
            {
                Syllabus = c.Content.Syllabus,
                LearningObjectives = c.Content.LearningObjectives,
                PreRequisites = c.Content.PreRequisites,
                TargetAudience = c.Content.TargetAudience,
                Language = c.Content.Language,
                Level = c.Content.Level
            },
            Price = c.Price,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            Lessons = c.Lessons.Select(l => new LessonDto
            {
                Id = l.Id,
                Title = l.Title,
                Content = l.Content,
                MaterialUrl = l.MaterialUrl,
                Order = l.Order,
                IsActive = true,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            }).ToList()
        }).ToList();
    }
} 