using MediatR;

namespace FluencyHub.Application.StudentManagement.Queries.GetStudentById;

public record GetStudentByIdQuery(Guid Id) : IRequest<StudentDto>;

public record StudentDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int EnrollmentsCount { get; init; }
    public int CertificatesCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
} 