using MediatR;

namespace FluencyHub.ContentManagement.Application.Commands.UpdateCourse;

public record UpdateCourseCommand : IRequest<bool>
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Syllabus { get; init; }
    public required string LearningObjectives { get; init; }
    public required string PreRequisites { get; init; }
    public required string TargetAudience { get; init; }
    public required string Language { get; init; }
    public required string Level { get; init; }
    public decimal Price { get; init; }
} 