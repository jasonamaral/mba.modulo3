using MediatR;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Application.Common.Exceptions;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Domain.Enums;
using FluencyHub.SharedKernel.Events.StudentManagement;
using FluencyHub.SharedKernel.Queries;
using Microsoft.Extensions.Logging;
using IDomainEventService = FluencyHub.SharedKernel.Events.IDomainEventService;
using IStudentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;

namespace FluencyHub.StudentManagement.Application.Commands.CompleteCourseForStudent;

public class CompleteCourseForStudentCommandHandler : IRequestHandler<CompleteCourseForStudentCommand, CompleteCourseForStudentResult>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ILearningRepository _learningRepository;
    private readonly IMediator _mediator;
    private readonly IDomainEventService _domainEventService;
    private readonly ILogger<CompleteCourseForStudentCommandHandler> _logger;

    public CompleteCourseForStudentCommandHandler(
        IStudentRepository studentRepository,
        IEnrollmentRepository enrollmentRepository,
        ILearningRepository learningRepository,
        IMediator mediator,
        IDomainEventService domainEventService,
        ILogger<CompleteCourseForStudentCommandHandler> logger)
    {
        _studentRepository = studentRepository;
        _enrollmentRepository = enrollmentRepository;
        _learningRepository = learningRepository;
        _mediator = mediator;
        _domainEventService = domainEventService;
        _logger = logger;
    }

    public async Task<CompleteCourseForStudentResult> Handle(CompleteCourseForStudentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processando conclusão do curso {CourseId} para o estudante {StudentId}", 
            request.CourseId, request.StudentId);

        // Verificar se o estudante existe
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        if (student == null)
        {
            throw new NotFoundException($"Estudante com ID {request.StudentId} não encontrado");
        }

        // Verificar se o curso existe
        var courseInfo = await _mediator.Send(new GetCourseById { CourseId = request.CourseId }, cancellationToken);
        if (courseInfo == null)
        {
            throw new NotFoundException($"Curso com ID {request.CourseId} não encontrado");
        }

        // Verificar se o estudante está matriculado no curso
        var enrollments = await _enrollmentRepository.GetByStudentIdAsync(request.StudentId);
        var enrollment = enrollments.FirstOrDefault(e => e.CourseId == request.CourseId && e.Status == StatusMatricula.Ativa);
        if (enrollment == null)
        {
            throw new InvalidOperationException($"Estudante não está matriculado no curso ou matrícula não está ativa");
        }

        // Obter ou criar o histórico de aprendizado
        var learningHistory = await _learningRepository.GetLearningHistoryByStudentIdAsync(request.StudentId);
        if (learningHistory == null)
        {
            learningHistory = new LearningHistory(request.StudentId);
            await _learningRepository.AddLearningHistoryAsync(learningHistory);
        }

        // Verificar se o curso já foi completado
        if (learningHistory.HasCompletedCourse(request.CourseId))
        {
            _logger.LogWarning("Curso {CourseId} já foi completado pelo estudante {StudentId}", 
                request.CourseId, request.StudentId);
            
            return new CompleteCourseForStudentResult
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                IsSuccessful = true,
                FinalScore = request.FinalScore,
                CertificateGenerated = false,
                CertificateId = null
            };
        }

        // Marcar o curso como completado
        learningHistory.CompleteCourse(request.CourseId);

        // Completar a matrícula
        enrollment.CompleteEnrollment();

        // Gerar certificado
        var certificate = await GenerateCertificate(student, courseInfo, request.FinalScore, cancellationToken);

        // Salvar alterações
        await _learningRepository.SaveChangesAsync(cancellationToken);

        // Disparar evento de conclusão do curso
        var courseCompletedEvent = new CourseCompletedEvent(
            request.StudentId, 
            request.CourseId, 
            request.CompletionDate, 
            request.FinalScore);
        
        await _domainEventService.PublishAsync(courseCompletedEvent);

        _logger.LogInformation("Curso {CourseId} completado com sucesso pelo estudante {StudentId}. Certificado {CertificateId} gerado.", 
            request.CourseId, request.StudentId, certificate?.Id);

        return new CompleteCourseForStudentResult
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            IsSuccessful = true,
            FinalScore = request.FinalScore,
            CertificateGenerated = certificate != null,
            CertificateId = certificate?.Id
        };
    }

    private async Task<Certificate?> GenerateCertificate(Student student, dynamic courseInfo, int? finalScore, CancellationToken cancellationToken)
    {
        try
        {
            var certificateTitle = $"Certificado de Conclusão - {courseInfo.Name}";
            
            var certificate = new Certificate(
                student.Id, 
                courseInfo.Id, 
                certificateTitle)
            {
                Student = student,
                Title = certificateTitle,
                Course = courseInfo
            };

            // Definir score se fornecido
            if (finalScore.HasValue)
            {
                certificate.SetScore(finalScore.Value);
            }

            // Adicionar certificado ao contexto (assumindo que existe um repositório de certificados)
            // Por enquanto, vamos apenas retornar o certificado criado
            // Em uma implementação completa, você salvaria no banco de dados

            _logger.LogInformation("Certificado {CertificateNumber} gerado para o estudante {StudentId} no curso {CourseId}", 
                certificate.CertificateNumber, student.Id, (Guid)courseInfo.Id);

            return certificate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar certificado para o estudante {StudentId} no curso {CourseId}", 
                student.Id, (Guid)courseInfo.Id);
            return null;
        }
    }
} 