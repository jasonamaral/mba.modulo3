using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.ContentManagement;
using FluencyHub.Domain.StudentManagement;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.GenerateCertificate;

public class GenerateCertificateCommandHandler : IRequestHandler<GenerateCertificateCommand, Guid>
{
    private readonly FluencyHub.Application.Common.Interfaces.IStudentRepository _studentRepository;
    private readonly FluencyHub.Application.Common.Interfaces.ICourseRepository _courseRepository;
    
    public GenerateCertificateCommandHandler(
        FluencyHub.Application.Common.Interfaces.IStudentRepository studentRepository,
        FluencyHub.Application.Common.Interfaces.ICourseRepository courseRepository)
    {
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
    }
    
    public async Task<Guid> Handle(GenerateCertificateCommand request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        
        if (student == null)
        {
            throw new NotFoundException(nameof(Student), request.StudentId);
        }
        
        var course = await _courseRepository.GetByIdAsync(request.CourseId);
        
        if (course == null)
        {
            throw new NotFoundException(nameof(Course), request.CourseId);
        }
        
        var enrollment = student.Enrollments.FirstOrDefault(e => e.CourseId == request.CourseId);
        
        if (enrollment == null)
        {
            throw new InvalidOperationException($"Student {request.StudentId} is not enrolled in course {request.CourseId}");
        }
        
        if (!enrollment.IsCompleted)
        {
            throw new InvalidOperationException("Cannot generate certificate for incomplete course");
        }
        
        var certificateTitle = $"{course.Name} - Certificate of Completion";
        student.AddCertificate(course.Id, certificateTitle);
        
        await _studentRepository.SaveChangesAsync(cancellationToken);
        
        return student.Certificates.Last().Id;
    }
} 