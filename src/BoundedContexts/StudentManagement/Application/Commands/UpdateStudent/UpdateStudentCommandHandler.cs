using MediatR;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Application.Common.Exceptions;

namespace FluencyHub.StudentManagement.Application.Commands.UpdateStudent;

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, bool>
{
    private readonly FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository _studentRepository;

    public UpdateStudentCommandHandler(FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<bool> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.Id);
        
        if (student == null)
            throw new NotFoundException($"Estudante com ID {request.Id} não encontrado");

        student.Update(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.Address,
            request.City,
            request.State,
            request.Country);

        await _studentRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
} 