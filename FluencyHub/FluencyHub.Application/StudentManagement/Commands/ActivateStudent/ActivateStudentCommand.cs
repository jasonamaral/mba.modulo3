using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.ActivateStudent;

public record ActivateStudentCommand(Guid Id) : IRequest; 