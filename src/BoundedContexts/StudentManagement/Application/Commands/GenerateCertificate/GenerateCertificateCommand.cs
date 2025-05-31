using MediatR;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Application.Common.Exceptions;
using FluencyHub.StudentManagement.Domain;
using IStudentRepositoryInterface = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;
using FluencyHub.SharedKernel.Contracts;
using FluencyHub.StudentManagement.Domain.Models;

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

        // Obter as informações do curso
        var courseInfo = await _courseRepository.GetByIdAsync(request.CourseId);
        if (courseInfo == null)
            throw new NotFoundException($"Course with ID {request.CourseId} not found");

        // Verificar se já existe um certificado para este aluno e curso
        var existingCertificate = await _certificateRepository.GetByStudentAndCourseAsync(request.StudentId, request.CourseId);
        if (existingCertificate != null)
            throw new InvalidOperationException($"Certificate already exists for student {request.StudentId} and course {request.CourseId}");

        // Criar CourseReference que implementa ICourse
        var courseReference = new CourseReference(
            courseInfo.Id,
            courseInfo.Name,
            courseInfo.Description,
            courseInfo.Price,
            true // assumindo que o curso está ativo se existe
        );

        // Gerar novo certificado
        var certificate = new Certificate(request.StudentId, request.CourseId, courseInfo.Name)
        {
            Student = student,
            Course = courseReference,
            Title = courseInfo.Name,
            CertificateNumber = $"CERT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}"
        };
        
        // Atualizar propriedades adicionais, se necessário
        if (request.Score.HasValue)
            certificate.SetScore(request.Score.Value);
            
        if (!string.IsNullOrEmpty(request.Feedback))
            certificate.SetFeedback(request.Feedback);

        await _certificateRepository.AddAsync(certificate);
        await _certificateRepository.SaveChangesAsync(cancellationToken);

        return certificate.Id;
    }
} 