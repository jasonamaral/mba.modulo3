using MediatR;

namespace FluencyHub.StudentManagement.Application.Commands.ActivateStudent;

public record ActivateStudentCommand : IRequest<bool>
{
    public required Guid Id { get; init; }
} 