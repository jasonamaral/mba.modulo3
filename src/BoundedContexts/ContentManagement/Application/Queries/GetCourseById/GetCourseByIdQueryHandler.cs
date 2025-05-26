using MediatR;
using FluencyHub.ContentManagement.Application.Common.Interfaces;
using FluencyHub.ContentManagement.Application.Common.Models;

namespace FluencyHub.ContentManagement.Application.Queries.GetCourseById;

public class GetCourseByIdQueryHandler : IRequestHandler<GetCourseByIdQuery, FluencyHub.ContentManagement.Application.Common.Models.CourseDto>
{
    private readonly ICourseRepository _courseRepository;

    public GetCourseByIdQueryHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<FluencyHub.ContentManagement.Application.Common.Models.CourseDto> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(request.CourseId);
        
        if (course == null)
        {
            throw new KeyNotFoundException($"Course with ID {request.CourseId} not found");
        }

        return new FluencyHub.ContentManagement.Application.Common.Models.CourseDto
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
        };
    }
} 