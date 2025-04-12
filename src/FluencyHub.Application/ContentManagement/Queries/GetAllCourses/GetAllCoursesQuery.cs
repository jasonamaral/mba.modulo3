using FluencyHub.Application.ContentManagement.Queries.GetCourseById;
using MediatR;

namespace FluencyHub.Application.ContentManagement.Queries.GetAllCourses;

public record GetAllCoursesQuery : IRequest<IEnumerable<CourseDto>>; 