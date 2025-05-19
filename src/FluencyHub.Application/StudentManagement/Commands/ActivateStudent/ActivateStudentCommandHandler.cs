using FluencyHub.Application.Common.Exceptions;
using FluencyHub.StudentManagement.Domain;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.ActivateStudent;

public class ActivateStudentCommandHandler : IRequestHandler<ActivateStudentCommand>
{
    private readonly FluencyHub.Application.Common.Interfaces.IStudentRepository _studentRepository;

    public ActivateStudentCommandHandler(FluencyHub.Application.Common.Interfaces.IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task Handle(ActivateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.Id);

        if (student == null)
        {
            throw new NotFoundException(nameof(Student), request.Id);
        }

        student.Activate();

        await _studentRepository.SaveChangesAsync(cancellationToken);
    }
}