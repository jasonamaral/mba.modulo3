using FluencyHub.StudentManagement.Application.Common.Exceptions;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Application.Queries.GetEnrollmentById;
using MediatR;
using IStudentRepositoryInterface = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;

namespace FluencyHub.StudentManagement.Application.Queries.GetStudentEnrollments;

public record GetStudentEnrollmentsQuery(Guid StudentId) : IRequest<IEnumerable<EnrollmentDto>>;

public class GetStudentEnrollmentsQueryHandler : IRequestHandler<GetStudentEnrollmentsQuery, IEnumerable<EnrollmentDto>>
{
    private readonly IStudentRepositoryInterface _studentRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public GetStudentEnrollmentsQueryHandler(
        IStudentRepositoryInterface studentRepository,
        IEnrollmentRepository enrollmentRepository)
    {
        _studentRepository = studentRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<IEnumerable<EnrollmentDto>> Handle(GetStudentEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        
        if (student == null)
            throw new NotFoundException($"Student with ID {request.StudentId} not found");

        var enrollments = await _studentRepository.GetEnrollmentsByStudentIdAsync(request.StudentId);
        
        var result = new List<EnrollmentDto>();
        
        foreach (var enrollment in enrollments)
        {
            result.Add(new EnrollmentDto
            {
                Id = enrollment.Id,
                StudentId = enrollment.StudentId,
                CourseId = enrollment.CourseId,
                StudentName = $"{student.FirstName} {student.LastName}",
                CourseName = "Course Name", // Idealmente, buscaria o nome do curso
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