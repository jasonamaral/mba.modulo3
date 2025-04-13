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
            _logger.LogInformation("Obtendo lista de todos os estudantes");
            
            var students = await _studentRepository.GetAllAsync();
            
            // Mapear para DTO
            var studentsDto = students.Select(s => new StudentDto
            {
                Id = s.Id,
                Name = s.FullName,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber ?? string.Empty,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            }).ToList();
            
            _logger.LogInformation("Retornando {Count} estudantes", studentsDto.Count);
            
            return studentsDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar estudantes");
            throw;
        }
    }
} 