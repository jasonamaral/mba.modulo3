using MediatR;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;

namespace FluencyHub.StudentManagement.Application.Commands.CreateStudent;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Guid>
{
    private readonly FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository _studentRepository;

    public CreateStudentCommandHandler(FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Guid> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = new Student(
            firstName: request.FirstName,
            lastName: request.LastName,
            email: request.Email,
            dateOfBirth: request.DateOfBirth)
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            LearningHistory = new LearningHistory(Guid.NewGuid())
        };

        // Usar o método Update para definir os campos opcionais
        student.Update(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.Address,
            request.City,
            request.State,
            request.Country);

        await _studentRepository.AddAsync(student);
        await _studentRepository.SaveChangesAsync(cancellationToken);

        return student.Id;
    }
} 