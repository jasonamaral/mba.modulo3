using FluencyHub.StudentManagement.Application.Common.Exceptions;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Application.Queries.GetStudentById;
using MediatR;
using IStudentRepositoryInterface = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;

namespace FluencyHub.StudentManagement.Application.Queries.GetStudentByEmail;

public record GetStudentByEmailQuery(string Email) : IRequest<StudentDto>;

public class GetStudentByEmailQueryHandler : IRequestHandler<GetStudentByEmailQuery, StudentDto>
{
    private readonly IStudentRepositoryInterface _studentRepository;

    public GetStudentByEmailQueryHandler(IStudentRepositoryInterface studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<StudentDto> Handle(GetStudentByEmailQuery request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByEmailAsync(request.Email);
        
        if (student == null)
            throw new NotFoundException($"Student with email {request.Email} not found");

        var enrollments = await _studentRepository.GetEnrollmentsByStudentIdAsync(student.Id);
        var certificates = await _studentRepository.GetCertificatesByStudentIdAsync(student.Id);

        return new StudentDto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            FullName = $"{student.FirstName} {student.LastName}",
            Email = student.Email,
            PhoneNumber = student.PhoneNumber,
            DateOfBirth = student.DateOfBirth,
            IsActive = student.IsActive,
            EnrollmentsCount = enrollments.Count(),
            CertificatesCount = certificates.Count(),
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt
        };
    }
} 