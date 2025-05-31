using MediatR;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Application.Common.Exceptions;

namespace FluencyHub.StudentManagement.Application.Queries.GetStudentById;

public class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, StudentDto>
{
    private readonly FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository _studentRepository;

    public GetStudentByIdQueryHandler(FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<StudentDto> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        
        if (student == null)
            throw new NotFoundException($"Estudante com ID {request.StudentId} não encontrado");

        return new StudentDto
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
        };
    }
} 