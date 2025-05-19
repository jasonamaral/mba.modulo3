using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.StudentManagement.Queries.GetStudentById;
using FluencyHub.StudentManagement.Domain;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Queries.GetStudentByEmail
{
    public class GetStudentByEmailQueryHandler : IRequestHandler<GetStudentByEmailQuery, StudentDto>
    {
        private readonly FluencyHub.Application.Common.Interfaces.IStudentRepository _studentRepository;

        public GetStudentByEmailQueryHandler(FluencyHub.Application.Common.Interfaces.IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<StudentDto> Handle(GetStudentByEmailQuery request, CancellationToken cancellationToken)
        {
            var student = await _studentRepository.GetByEmailAsync(request.Email);

            return student == null
                ? throw new NotFoundException(nameof(Student), request.Email)
                : new StudentDto
                {
                    Id = student.Id,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    FullName = student.FullName,
                    Email = student.Email,
                    DateOfBirth = student.DateOfBirth,
                    PhoneNumber = student.PhoneNumber ?? "",
                    IsActive = student.IsActive,
                    EnrollmentsCount = student.Enrollments.Count,
                    CertificatesCount = student.Certificates.Count,
                    CreatedAt = student.CreatedAt,
                    UpdatedAt = student.UpdatedAt
                };
        }
    }
}