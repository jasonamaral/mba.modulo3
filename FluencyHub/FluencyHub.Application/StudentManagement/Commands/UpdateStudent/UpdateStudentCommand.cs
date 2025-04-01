using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.UpdateStudent;

public record UpdateStudentCommand : IRequest
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
} 