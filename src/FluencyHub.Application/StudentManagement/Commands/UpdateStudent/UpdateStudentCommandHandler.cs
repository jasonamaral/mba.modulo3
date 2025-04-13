using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.StudentManagement;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.UpdateStudent;

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand>
{
    private readonly FluencyHub.Application.Common.Interfaces.IStudentRepository _studentRepository;
    
    public UpdateStudentCommandHandler(FluencyHub.Application.Common.Interfaces.IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }
    
    public async Task Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.Id);
        
        if (student == null)
        {
            throw new NotFoundException(nameof(Student), request.Id);
        }
        
        student.UpdateDetails(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.PhoneNumber);
            
        await _studentRepository.SaveChangesAsync(cancellationToken);
    }
} 