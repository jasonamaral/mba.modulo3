using MediatR;

namespace FluencyHub.StudentManagement.Application.Commands.EnrollStudent;

public record EnrollStudentCommand : IRequest<Guid>
{
    public required Guid StudentId { get; init; }
    public required Guid CourseId { get; init; }
    public required DateTime EnrollmentDate { get; init; }
    public decimal? DiscountPercentage { get; init; }
} 