using MediatR;
using FluencyHub.ContentManagement.Application.Common.Models;

namespace FluencyHub.ContentManagement.Application.Queries.GetLessonsByCourse;

public record GetLessonsByCourseQuery : IRequest<IEnumerable<LessonDto>>
{
    public required Guid CourseId { get; init; }
    public bool IncludeInactive { get; init; } = false;
} 