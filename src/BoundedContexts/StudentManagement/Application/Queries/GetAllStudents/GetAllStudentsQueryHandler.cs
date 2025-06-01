using MediatR;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Application.Common.Models;

namespace FluencyHub.StudentManagement.Application.Queries.GetAllStudents;

public class GetAllStudentsQueryHandler : IRequestHandler<GetAllStudentsQuery, IEnumerable<StudentDto>>
{
    private readonly IStudentRepository _studentRepository;

    public GetAllStudentsQueryHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<IEnumerable<StudentDto>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        var students = await _studentRepository.GetAllAsync(includeInactive: false, cancellationToken);
        
        return students.Select(student => new StudentDto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Email = student.Email,
            PhoneNumber = student.PhoneNumber ?? string.Empty,
            DateOfBirth = student.DateOfBirth,
            Address = student.Address,
            City = student.City,
            State = student.State,
            Country = student.Country,
            PostalCode = student.PostalCode,
            IsActive = student.IsActive,
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt,
            FullName = student.FullName,
            EnrollmentsCount = student.Enrollments.Count,
            CertificatesCount = student.Certificates.Count
        });
    }
} 