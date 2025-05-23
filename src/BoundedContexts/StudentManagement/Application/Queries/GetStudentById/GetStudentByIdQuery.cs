using MediatR;

namespace FluencyHub.StudentManagement.Application.Queries.GetStudentById;

public record GetStudentByIdQuery : IRequest<StudentDto>
{
    public required Guid StudentId { get; init; }
} 