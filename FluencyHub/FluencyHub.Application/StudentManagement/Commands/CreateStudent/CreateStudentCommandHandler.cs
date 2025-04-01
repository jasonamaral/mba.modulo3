using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.StudentManagement;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.CreateStudent;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Guid>
{
    private readonly FluencyHub.Application.Common.Interfaces.IStudentRepository _studentRepository;
    
    public CreateStudentCommandHandler(FluencyHub.Application.Common.Interfaces.IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }
    
    public async Task<Guid> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = new Student(
            request.FirstName,
            request.LastName,
            request.Email,
            request.DateOfBirth,
            request.PhoneNumber);
            
        await _studentRepository.AddAsync(student);
        await _studentRepository.SaveChangesAsync(cancellationToken);
        
        return student.Id;
    }
} 