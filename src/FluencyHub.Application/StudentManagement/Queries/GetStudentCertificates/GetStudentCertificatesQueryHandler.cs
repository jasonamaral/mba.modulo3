using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.StudentManagement.Queries.GetCertificateById;
using FluencyHub.StudentManagement.Domain;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Queries.GetStudentCertificates;

public class GetStudentCertificatesQueryHandler : IRequestHandler<GetStudentCertificatesQuery, IEnumerable<CertificateDto>>
{
    private readonly FluencyHub.Application.Common.Interfaces.IStudentRepository _studentRepository;
    private readonly FluencyHub.Application.Common.Interfaces.ICourseRepository _courseRepository;

    public GetStudentCertificatesQueryHandler(
        FluencyHub.Application.Common.Interfaces.IStudentRepository studentRepository,
        FluencyHub.Application.Common.Interfaces.ICourseRepository courseRepository)
    {
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
    }

    public async Task<IEnumerable<CertificateDto>> Handle(GetStudentCertificatesQuery request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId) ?? throw new NotFoundException(nameof(Student), request.StudentId);
        var result = new List<CertificateDto>();

        foreach (var certificate in student.Certificates)
        {
            var course = await _courseRepository.GetByIdAsync(certificate.CourseId);

            result.Add(new CertificateDto
            {
                Id = certificate.Id,
                StudentId = student.Id,
                StudentName = student.FullName,
                CourseId = certificate.CourseId,
                CourseName = course?.Name!,
                Title = certificate.Title,
                IssueDate = certificate.IssueDate
            });
        }

        return result;
    }
}