using MediatR;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;
using Microsoft.Extensions.Logging;

namespace FluencyHub.StudentManagement.Application.Commands.CreateStudent;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Guid>
{
    private readonly FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository _studentRepository;
    private readonly IIdentityService _identityService;
    private readonly ILogger<CreateStudentCommandHandler> _logger;

    public CreateStudentCommandHandler(
        FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository studentRepository,
        IIdentityService identityService,
        ILogger<CreateStudentCommandHandler> logger)
    {
        _studentRepository = studentRepository;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new student with email: {Email}", request.Email);

        // Criar o estudante no domínio
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

        _logger.LogInformation("Student created successfully with ID: {StudentId}", student.Id);

        try
        {
            // Criar o usuário de identidade usando o serviço
            var authResult = await _identityService.RegisterUserAsync(
                request.Email, 
                request.Password, 
                request.FirstName, 
                request.LastName);

            if (!authResult.Succeeded)
            {
                _logger.LogError("Failed to create user for student {StudentId}: {Errors}", 
                    student.Id, string.Join(", ", authResult.Errors));
                
                throw new InvalidOperationException($"Falha ao criar usuário: {string.Join(", ", authResult.Errors)}");
            }

            _logger.LogInformation("User created successfully for student {StudentId}", student.Id);

            // Atualizar o usuário com o StudentId
            var updateResult = await _identityService.UpdateUserStudentIdAsync(request.Email, student.Id);
            if (!updateResult)
            {
                _logger.LogWarning("Failed to update user with StudentId for {Email}", request.Email);
            }
            else
            {
                _logger.LogInformation("StudentId updated successfully for user {Email}", request.Email);
            }

            // Garantir que a role "Student" existe e atribuí-la ao usuário
            const string studentRole = "Student";
            await _identityService.EnsureRoleExistsAsync(studentRole);
            
            var roleResult = await _identityService.AddToRoleAsync(request.Email, studentRole);
            if (!roleResult)
            {
                _logger.LogWarning("Failed to add Student role to user {Email}", request.Email);
            }
            else
            {
                _logger.LogInformation("Student role assigned successfully to user {Email}", request.Email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user for student {StudentId}", student.Id);
            
            // Reverter a criação do estudante se falhar na criação do usuário
            try
            {
                await _studentRepository.DeleteAsync(student.Id);
            }
            catch (Exception deleteEx)
            {
                _logger.LogError(deleteEx, "Failed to rollback student creation for {StudentId}", student.Id);
            }
            
            throw;
        }

        return student.Id;
    }
} 