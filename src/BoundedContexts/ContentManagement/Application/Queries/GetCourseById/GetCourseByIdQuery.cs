using MediatR;
using FluencyHub.ContentManagement.Application.Common.Models;

namespace FluencyHub.ContentManagement.Application.Queries.GetCourseById;

public record GetCourseByIdQuery : IRequest<FluencyHub.ContentManagement.Application.Common.Models.CourseDto>
{
    public required Guid CourseId { get; init; }
} 