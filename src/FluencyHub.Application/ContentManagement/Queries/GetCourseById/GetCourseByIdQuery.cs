using MediatR;

namespace FluencyHub.Application.ContentManagement.Queries.GetCourseById;

public record GetCourseByIdQuery(Guid CourseId) : IRequest<CourseDto>; 