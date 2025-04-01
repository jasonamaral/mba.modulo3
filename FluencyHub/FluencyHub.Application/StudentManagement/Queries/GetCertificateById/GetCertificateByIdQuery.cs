using MediatR;

namespace FluencyHub.Application.StudentManagement.Queries.GetCertificateById;

public record GetCertificateByIdQuery(Guid Id) : IRequest<CertificateDto>;

public record CertificateDto
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public Guid CourseId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public DateTime IssueDate { get; init; }
} 