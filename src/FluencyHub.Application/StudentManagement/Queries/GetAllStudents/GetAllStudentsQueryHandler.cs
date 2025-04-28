using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.Application.StudentManagement.Queries.GetAllStudents;

public class GetAllStudentsQueryHandler : IRequestHandler<GetAllStudentsQuery, IEnumerable<StudentDto>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<GetAllStudentsQueryHandler> _logger;

    public GetAllStudentsQueryHandler(
        IStudentRepository studentRepository,
        ILogger<GetAllStudentsQueryHandler> logger)
    {
        _studentRepository = studentRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<StudentDto>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var students = await _studentRepository.GetAllAsync(request.IncludeInactive, cancellationToken);

            var studentDtos = students?.Select(s => new StudentDto
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
                Phone = s.PhoneNumber ?? string.Empty,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            ?? [];

            return studentDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching students");
            throw new BadRequestException(ex.Message);
        }
    }
}