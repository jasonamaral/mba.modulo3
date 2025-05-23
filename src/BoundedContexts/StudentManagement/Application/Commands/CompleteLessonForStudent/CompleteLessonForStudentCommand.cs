using MediatR;

namespace FluencyHub.StudentManagement.Application.Commands.CompleteLessonForStudent;

public record CompleteLessonForStudentCommand : IRequest<CompleteLessonForStudentResult>
{
    public required Guid StudentId { get; init; }
    public required Guid LessonId { get; init; }
    public required DateTime CompletionDate { get; init; }
    public int? Score { get; init; }
    public string? Feedback { get; init; }
} 