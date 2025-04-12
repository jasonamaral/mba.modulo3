using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.StudentManagement;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.DeactivateStudent;

public class DeactivateStudentCommandHandler : IRequestHandler<DeactivateStudentCommand>
{
    private readonly FluencyHub.Application.Common.Interfaces.IStudentRepository _studentRepository;
    
    public DeactivateStudentCommandHandler(FluencyHub.Application.Common.Interfaces.IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }
    
    public async Task Handle(DeactivateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.Id);
        
        if (student == null)
        {
            throw new NotFoundException(nameof(Student), request.Id);
        }
        
        student.Deactivate();
        
        await _studentRepository.SaveChangesAsync(cancellationToken);
    }
} 