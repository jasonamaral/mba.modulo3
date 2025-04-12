using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.ContentManagement;
using FluencyHub.Domain.StudentManagement;
using MediatR;
using System.Linq;

namespace FluencyHub.Application.StudentManagement.Queries.GetCertificateById;

public class GetCertificateByIdQueryHandler : IRequestHandler<GetCertificateByIdQuery, CertificateDto>
{
    private readonly FluencyHub.Application.Common.Interfaces.IStudentRepository _studentRepository;
    private readonly FluencyHub.Application.Common.Interfaces.ICourseRepository _courseRepository;
    
    public GetCertificateByIdQueryHandler(
        FluencyHub.Application.Common.Interfaces.IStudentRepository studentRepository,
        FluencyHub.Application.Common.Interfaces.ICourseRepository courseRepository)
    {
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
    }
    
    public async Task<CertificateDto> Handle(GetCertificateByIdQuery request, CancellationToken cancellationToken)
    {
        // Find the student that has this certificate
        var students = await _studentRepository.GetAllAsync();
        var studentWithCertificate = students.FirstOrDefault(s => 
            s.Certificates.Any(c => c.Id == request.Id));
        
        if (studentWithCertificate == null)
        {
            throw new NotFoundException(nameof(Certificate), request.Id);
        }
        
        var certificate = studentWithCertificate.Certificates.First(c => c.Id == request.Id);
        var course = await _courseRepository.GetByIdAsync(certificate.CourseId);
        
        if (course == null)
        {
            throw new NotFoundException(nameof(Course), certificate.CourseId);
        }
        
        return new CertificateDto
        {
            Id = certificate.Id,
            StudentId = studentWithCertificate.Id,
            StudentName = studentWithCertificate.FullName,
            CourseId = certificate.CourseId,
            CourseName = course.Name,
            Title = certificate.Title,
            IssueDate = certificate.IssueDate
        };
    }
} 