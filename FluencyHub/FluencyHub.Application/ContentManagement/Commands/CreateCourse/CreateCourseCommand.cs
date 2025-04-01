using FluencyHub.Application.Common.Interfaces;
using MediatR;

namespace FluencyHub.Application.ContentManagement.Commands.CreateCourse;

public record CreateCourseCommand : IRequest<Guid>
{
    public string Name { get; init; }
    public string Description { get; init; }
    public string Syllabus { get; init; }
    public string LearningObjectives { get; init; }
    public string PreRequisites { get; init; }
    public string TargetAudience { get; init; }
    public string Language { get; init; }
    public string Level { get; init; }
    public decimal Price { get; init; }
} 