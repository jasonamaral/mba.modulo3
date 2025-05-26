using MediatR;
using FluencyHub.ContentManagement.Application.Common.Models;

namespace FluencyHub.ContentManagement.Application.Queries.GetCourseById;

public record GetCourseByIdQuery : IRequest<CourseDto>
{
    public required Guid CourseId { get; init; }
} 