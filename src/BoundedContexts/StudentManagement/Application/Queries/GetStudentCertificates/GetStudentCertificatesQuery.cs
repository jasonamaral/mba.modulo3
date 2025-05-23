using FluencyHub.StudentManagement.Application.Common.Exceptions;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Application.Queries.GetCertificateById;
using MediatR;

namespace FluencyHub.StudentManagement.Application.Queries.GetStudentCertificates;

public record GetStudentCertificatesQuery(Guid StudentId) : IRequest<IEnumerable<CertificateDto>>;

public class GetStudentCertificatesQueryHandler : IRequestHandler<GetStudentCertificatesQuery, IEnumerable<CertificateDto>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ICertificateRepository _certificateRepository;

    public GetStudentCertificatesQueryHandler(
        IStudentRepository studentRepository,
        ICertificateRepository certificateRepository)
    {
        _studentRepository = studentRepository;
        _certificateRepository = certificateRepository;
    }

    public async Task<IEnumerable<CertificateDto>> Handle(GetStudentCertificatesQuery request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        if (student == null)
            throw new NotFoundException($"Student with ID {request.StudentId} not found");

        var certificates = await _certificateRepository.GetByStudentIdAsync(request.StudentId);
        return certificates.Select(c => new CertificateDto
        {
            Id = c.Id,
            StudentId = c.StudentId,
            CourseId = c.CourseId,
            StudentName = $"{student.FirstName} {student.LastName}",
            CourseName = "Course Name", // Aqui seria ideal obter o nome do curso, mas estamos em um contexto limitado
            IssueDate = c.IssueDate,
            CertificateNumber = c.CertificateNumber,
            Score = c.Score,
            Feedback = c.Feedback
        });
    }
} 