using MediatR;

namespace FluencyHub.Application.StudentManagement.Queries.GetEnrollmentById;

public record GetEnrollmentByIdQuery(Guid Id) : IRequest<EnrollmentDto>;

public record EnrollmentDto
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public Guid CourseId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime EnrollmentDate { get; init; }
    public DateTime? ActivationDate { get; init; }
    public DateTime? CompletionDate { get; init; }
} 