using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.StudentManagement.Queries.GetEnrollmentById;
using FluencyHub.Domain.StudentManagement;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Queries.GetStudentEnrollments;

public class GetStudentEnrollmentsQueryHandler : IRequestHandler<GetStudentEnrollmentsQuery, IEnumerable<EnrollmentDto>>
{
    private readonly FluencyHub.Application.Common.Interfaces.IStudentRepository _studentRepository;
    private readonly FluencyHub.Application.Common.Interfaces.ICourseRepository _courseRepository;
    
    public GetStudentEnrollmentsQueryHandler(
        FluencyHub.Application.Common.Interfaces.IStudentRepository studentRepository,
        FluencyHub.Application.Common.Interfaces.ICourseRepository courseRepository)
    {
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
    }
    
    public async Task<IEnumerable<EnrollmentDto>> Handle(GetStudentEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        
        if (student == null)
        {
            throw new NotFoundException(nameof(Student), request.StudentId);
        }
        
        var result = new List<EnrollmentDto>();
        
        foreach (var enrollment in student.Enrollments)
        {
            var course = await _courseRepository.GetByIdAsync(enrollment.CourseId);
            
            result.Add(new EnrollmentDto
            {
                Id = enrollment.Id,
                StudentId = enrollment.StudentId,
                CourseId = enrollment.CourseId,
                CourseName = course?.Name ?? "Unknown Course",
                Price = enrollment.Price,
                Status = enrollment.Status.ToString(),
                EnrollmentDate = enrollment.EnrollmentDate,
                ActivationDate = enrollment.ActivationDate,
                CompletionDate = enrollment.CompletionDate
            });
        }
        
        return result;
    }
} 