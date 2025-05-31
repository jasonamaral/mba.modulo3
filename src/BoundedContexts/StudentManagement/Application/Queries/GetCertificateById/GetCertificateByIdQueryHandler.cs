using MediatR;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Application.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace FluencyHub.StudentManagement.Application.Queries.GetCertificateById;

public class GetCertificateByIdQueryHandler : IRequestHandler<GetCertificateByIdQuery, CertificateDto>
{
    private readonly ICertificateRepository _certificateRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<GetCertificateByIdQueryHandler> _logger;

    public GetCertificateByIdQueryHandler(
        ICertificateRepository certificateRepository,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository,
        ILogger<GetCertificateByIdQueryHandler> logger)
    {
        _certificateRepository = certificateRepository;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
        _logger = logger;
    }

    public async Task<CertificateDto> Handle(GetCertificateByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando certificado com ID {CertificateId}", request.CertificateId);

        var certificate = await _certificateRepository.GetByIdAsync(request.CertificateId);
        if (certificate == null)
        {
            _logger.LogWarning("Certificado com ID {CertificateId} não encontrado", request.CertificateId);
            throw new NotFoundException($"Certificate with ID {request.CertificateId} not found");
        }

        // Obter informações do estudante
        var student = await _studentRepository.GetByIdAsync(certificate.StudentId);
        if (student == null)
        {
            _logger.LogError("Estudante com ID {StudentId} não encontrado para o certificado {CertificateId}", 
                certificate.StudentId, request.CertificateId);
            throw new NotFoundException($"Student with ID {certificate.StudentId} not found");
        }

        // Obter informações do curso
        var courseInfo = await _courseRepository.GetByIdAsync(certificate.CourseId);
        var courseName = courseInfo?.Name ?? "Curso não encontrado";

        _logger.LogInformation("Certificado {CertificateId} encontrado para o estudante {StudentId}", 
            request.CertificateId, certificate.StudentId);

        return new CertificateDto
        {
            Id = certificate.Id,
            StudentId = certificate.StudentId,
            CourseId = certificate.CourseId,
            StudentName = $"{student.FirstName} {student.LastName}",
            CourseName = courseName,
            IssueDate = certificate.IssueDate,
            CertificateNumber = certificate.CertificateNumber,
            Score = certificate.Score,
            Feedback = certificate.Feedback
        };
    }
} 