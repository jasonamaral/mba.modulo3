using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.StudentManagement;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Queries.GetStudentById;

public class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, StudentDto>
{
    private readonly FluencyHub.Application.Common.Interfaces.IStudentRepository _studentRepository;
    
    public GetStudentByIdQueryHandler(FluencyHub.Application.Common.Interfaces.IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }
    
    public async Task<StudentDto> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.Id);
        
        if (student == null)
        {
            throw new NotFoundException(nameof(Student), request.Id);
        }
        
        return new StudentDto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            FullName = student.FullName,
            Email = student.Email,
            DateOfBirth = student.DateOfBirth,
            PhoneNumber = student.PhoneNumber,
            IsActive = student.IsActive,
            EnrollmentsCount = student.Enrollments.Count,
            CertificatesCount = student.Certificates.Count,
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt
        };
    }
} 