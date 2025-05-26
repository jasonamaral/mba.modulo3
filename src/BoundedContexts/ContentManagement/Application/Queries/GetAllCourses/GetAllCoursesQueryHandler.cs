using MediatR;
using FluencyHub.ContentManagement.Application.Common.Models;
using FluencyHub.ContentManagement.Domain;

namespace FluencyHub.ContentManagement.Application.Queries.GetAllCourses;

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
        
        return courses.Select(course => new CourseDto
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description,
            Syllabus = course.Content.Syllabus,
            LearningObjectives = course.Content.LearningObjectives,
            PreRequisites = course.Content.PreRequisites,
            TargetAudience = course.Content.TargetAudience,
            Language = course.Content.Language,
            Level = course.Content.Level,
            Price = course.Price,
            IsActive = course.IsActive,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt
        });
    }
} 