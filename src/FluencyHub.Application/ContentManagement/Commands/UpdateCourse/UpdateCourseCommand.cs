using MediatR;

namespace FluencyHub.Application.ContentManagement.Commands.UpdateCourse;

public record UpdateCourseCommand : IRequest<bool>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Syllabus { get; init; } = string.Empty;
    public string LearningObjectives { get; init; } = string.Empty;
    public string PreRequisites { get; init; } = string.Empty;
    public string TargetAudience { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public string Level { get; init; } = string.Empty;
    public decimal Price { get; init; }
} 