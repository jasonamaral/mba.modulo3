using MediatR;

namespace FluencyHub.StudentManagement.Application.Queries.GetStudentProgress;

public record GetStudentProgressQuery : IRequest<StudentProgressViewModel>
{
    public required Guid StudentId { get; init; }
} 