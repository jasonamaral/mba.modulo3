using MediatR;

namespace FluencyHub.StudentManagement.Application.Queries.GetEnrollmentById;

public record GetEnrollmentByIdQuery : IRequest<EnrollmentDto>
{
    public required Guid EnrollmentId { get; init; }
} 