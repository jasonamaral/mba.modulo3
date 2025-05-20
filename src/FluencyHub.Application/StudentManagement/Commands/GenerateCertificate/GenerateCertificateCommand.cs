using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.GenerateCertificate;

public record GenerateCertificateCommand : IRequest<Guid>
{
    public required Guid StudentId { get; init; }
    public required Guid CourseId { get; init; }
    public required DateTime IssueDate { get; init; }
    public int? Score { get; init; }
    public string? Feedback { get; init; }
} 