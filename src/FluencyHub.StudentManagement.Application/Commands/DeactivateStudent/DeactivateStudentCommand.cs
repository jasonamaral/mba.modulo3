using MediatR;

namespace FluencyHub.StudentManagement.Application.Commands.DeactivateStudent;

public record DeactivateStudentCommand : IRequest<bool>
{
    public required Guid Id { get; init; }
} 