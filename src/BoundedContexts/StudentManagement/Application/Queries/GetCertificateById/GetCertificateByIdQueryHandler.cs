using MediatR;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.SharedKernel.Common.Exceptions;

namespace FluencyHub.StudentManagement.Application.Queries.GetCertificateById;

public class GetCertificateByIdQueryHandler : IRequestHandler<GetCertificateByIdQuery, CertificateDto>
{
    private readonly ICertificateRepository _certificateRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;

    public GetCertificateByIdQueryHandler(
        ICertificateRepository certificateRepository,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository)
    {
        _certificateRepository = certificateRepository;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
    }

    public async Task<CertificateDto> Handle(GetCertificateByIdQuery request, CancellationToken cancellationToken)
    {
        var certificate = await _certificateRepository.GetByIdAsync(request.CertificateId);
        if (certificate == null)
        {
            throw new NotFoundException($"Certificado com ID {request.CertificateId} não encontrado");
        }

        // Obter informações do estudante
        var student = await _studentRepository.GetByIdAsync(certificate.StudentId);
        if (student == null)
        {
            throw new NotFoundException($"Estudante com ID {certificate.StudentId} não encontrado");
        }

        // Obter informações do curso
        var courseInfo = await _courseRepository.GetByIdAsync(certificate.CourseId);
        var courseName = courseInfo?.Name ?? "Curso não encontrado";

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