using MediatR;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Application.Common.Exceptions;
using FluencyHub.StudentManagement.Domain;
using IStudentRepositoryInterface = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;

namespace FluencyHub.StudentManagement.Application.Commands.GenerateCertificate;

public record GenerateCertificateCommand : IRequest<Guid>
{
    public required Guid StudentId { get; init; }
    public required Guid CourseId { get; init; }
    public required DateTime IssueDate { get; init; }
    public int? Score { get; init; }
    public string? Feedback { get; init; }
}

public class GenerateCertificateCommandHandler : IRequestHandler<GenerateCertificateCommand, Guid>
{
    private readonly IStudentRepositoryInterface _studentRepository;
    private readonly ICertificateRepository _certificateRepository;
    private readonly ICourseRepository _courseRepository;

    public GenerateCertificateCommandHandler(
        IStudentRepositoryInterface studentRepository,
        ICertificateRepository certificateRepository,
        ICourseRepository courseRepository)
    {
        _studentRepository = studentRepository;
        _certificateRepository = certificateRepository;
        _courseRepository = courseRepository;
    }

    public async Task<Guid> Handle(GenerateCertificateCommand request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        if (student == null)
            throw new NotFoundException($"Student with ID {request.StudentId} not found");

        // Verificar se o curso existe usando o adaptador
        var courseExists = await _courseRepository.ExistsAsync(request.CourseId);
        if (!courseExists)
            throw new NotFoundException($"Course with ID {request.CourseId} not found");

        // Obter o nome do curso para o certificado
        var courseName = await _courseRepository.GetNameAsync(request.CourseId);

        // Verificar se já existe um certificado para este aluno e curso
        var existingCertificate = await _certificateRepository.GetByStudentAndCourseAsync(request.StudentId, request.CourseId);
        if (existingCertificate != null)
            throw new InvalidOperationException($"Certificate already exists for student {request.StudentId} and course {request.CourseId}");

        // Gerar novo certificado
        var certificate = new Certificate(request.StudentId, request.CourseId, courseName);
        
        // Atualizar propriedades adicionais, se necessário
        // Nota: Estamos assumindo que há algum método para adicionar score e feedback na classe Certificate

        await _certificateRepository.AddAsync(certificate);
        await _certificateRepository.SaveChangesAsync(cancellationToken);

        return certificate.Id;
    }
} 