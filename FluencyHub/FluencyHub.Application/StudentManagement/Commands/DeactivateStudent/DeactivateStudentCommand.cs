using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.DeactivateStudent;

public record DeactivateStudentCommand(Guid Id) : IRequest; 