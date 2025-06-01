using MediatR;

namespace FluencyHub.StudentManagement.Application.Commands.CompleteCourseForStudent;

public record CompleteCourseForStudentCommand : IRequest<CompleteCourseForStudentResult>
{
    public required Guid StudentId { get; init; }
    public required Guid CourseId { get; init; }
    public required DateTime CompletionDate { get; init; }
    public int? FinalScore { get; init; }
    public string? Feedback { get; init; }
} 