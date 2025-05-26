using MediatR;
using FluencyHub.StudentManagement.Application.Common.Exceptions;
using FluencyHub.SharedKernel.Queries;
using Microsoft.Extensions.Logging;
using IStudentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;
using IEnrollmentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IEnrollmentRepository;

namespace FluencyHub.StudentManagement.Application.Queries.GetEnrollmentById;

public class GetEnrollmentByIdQueryHandler : IRequestHandler<GetEnrollmentByIdQuery, EnrollmentDto>
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<GetEnrollmentByIdQueryHandler> _logger;

    public GetEnrollmentByIdQueryHandler(
        IEnrollmentRepository enrollmentRepository,
        IStudentRepository studentRepository,
        IMediator mediator,
        ILogger<GetEnrollmentByIdQueryHandler> logger)
    {
        _enrollmentRepository = enrollmentRepository;
        _studentRepository = studentRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<EnrollmentDto> Handle(GetEnrollmentByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting enrollment with ID {EnrollmentId}", request.EnrollmentId);

        var enrollment = await _enrollmentRepository.GetByIdAsync(request.EnrollmentId);
        if (enrollment == null)
        {
            throw new NotFoundException($"Enrollment with ID {request.EnrollmentId} not found");
        }

        // Obter informações do estudante
        var student = await _studentRepository.GetByIdAsync(enrollment.StudentId);
        if (student == null)
        {
            throw new NotFoundException($"Student with ID {enrollment.StudentId} not found");
        }

        // Obter informações do curso usando query compartilhada
        string courseName = "Course Name"; // Valor padrão
        try
        {
            var courseInfo = await _mediator.Send(new GetCourseById { CourseId = enrollment.CourseId }, cancellationToken);
            if (courseInfo != null)
            {
                courseName = courseInfo.Name;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get course information for course {CourseId}", enrollment.CourseId);
        }

        // Calcular preço final considerando desconto
        var finalPrice = enrollment.Price;
        decimal? discountPercentage = null;

        // Calcular progresso (simplificado por enquanto)
        int? progress = null;
        try
        {
            // Em uma implementação completa, isso seria calculado baseado no progresso real do curso
            progress = enrollment.Status.ToString() == "Ativa" ? 0 : 
                      enrollment.Status.ToString() == "Concluida" ? 100 : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not calculate progress for enrollment {EnrollmentId}", enrollment.Id);
        }

        return new EnrollmentDto
        {
            Id = enrollment.Id,
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId,
            StudentName = $"{student.FirstName} {student.LastName}",
            CourseName = courseName,
            EnrollmentDate = enrollment.EnrollmentDate,
            Price = enrollment.Price,
            DiscountPercentage = discountPercentage,
            FinalPrice = finalPrice,
            IsCompleted = enrollment.CompletionDate.HasValue,
            CompletionDate = enrollment.CompletionDate,
            Progress = progress,
            Status = enrollment.Status.ToString(),
            ActivationDate = enrollment.ActivationDate
        };
    }
} 