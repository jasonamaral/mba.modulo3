using MediatR;
using FluencyHub.ContentManagement.Application.Common.Models;

namespace FluencyHub.ContentManagement.Application.Queries.GetAllCourses;

public record GetAllCoursesQuery : IRequest<IEnumerable<CourseDto>>
{
    public bool IncludeInactive { get; init; } = false;
} 