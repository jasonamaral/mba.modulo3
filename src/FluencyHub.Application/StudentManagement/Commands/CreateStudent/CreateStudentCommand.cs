using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.CreateStudent;

public record CreateStudentCommand : IRequest<Guid>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
} 