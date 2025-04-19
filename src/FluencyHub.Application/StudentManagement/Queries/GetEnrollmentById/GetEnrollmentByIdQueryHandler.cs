using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.StudentManagement;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Queries.GetEnrollmentById;

public class GetEnrollmentByIdQueryHandler : IRequestHandler<GetEnrollmentByIdQuery, EnrollmentDto>
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICourseRepository _courseRepository;
    
    public GetEnrollmentByIdQueryHandler(
        IEnrollmentRepository enrollmentRepository,
        ICourseRepository courseRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _courseRepository = courseRepository;
    }
    
    public async Task<EnrollmentDto> Handle(GetEnrollmentByIdQuery request, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(request.Id) ?? throw new NotFoundException(nameof(Enrollment), request.Id);
        var course = await _courseRepository.GetByIdAsync(enrollment.CourseId);
        
        return new EnrollmentDto
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
        };
    }
} 