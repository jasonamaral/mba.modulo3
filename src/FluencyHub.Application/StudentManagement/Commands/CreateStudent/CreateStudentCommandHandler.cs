using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.StudentManagement;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.CreateStudent;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Guid>
{
    private readonly FluencyHub.Application.Common.Interfaces.IStudentRepository _studentRepository;
    private readonly IIdentityService _identityService;
    
    public CreateStudentCommandHandler(FluencyHub.Application.Common.Interfaces.IStudentRepository studentRepository, IIdentityService identityService)
    {
        _studentRepository = studentRepository;
        _identityService = identityService;
    }
    
    public async Task<Guid> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        // Primeiro registra o usuário no Identity
        var authResult = await _identityService.RegisterUserAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);
            
        if (!authResult.Succeeded)
        {
            throw new ApplicationException($"Failed to create user for student: {string.Join(", ", authResult.Errors)}");
        }

        try
        {
            // Depois cria o estudante
            var student = new Student(
                request.FirstName,
                request.LastName,
                request.Email,
                request.DateOfBirth,
                request.PhoneNumber);
                
            await _studentRepository.AddAsync(student);
            await _studentRepository.SaveChangesAsync(cancellationToken);

            // Atualiza o ID do estudante no usuário
            await _identityService.UpdateUserStudentIdAsync(request.Email, student.Id);
            
            return student.Id;
        }
        catch (Exception)
        {
            // Se algo der errado na criação do estudante, remove o usuário criado
            await _identityService.DeleteUserAsync(request.Email);
            throw;
        }
    }
} 