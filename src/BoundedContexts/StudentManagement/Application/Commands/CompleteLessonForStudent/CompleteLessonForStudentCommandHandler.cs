using MediatR;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Application.Common.Exceptions;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Domain.Enums;
using FluencyHub.SharedKernel.Events.StudentManagement;
using FluencyHub.SharedKernel.Queries;
using FluencyHub.ContentManagement.Application.Queries.GetLessonsByCourseId;
using FluencyHub.ContentManagement.Application.Queries.GetLessonById;
using Microsoft.Extensions.Logging;
using IDomainEventService = FluencyHub.SharedKernel.Events.IDomainEventService;
using IStudentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;

namespace FluencyHub.StudentManagement.Application.Commands.CompleteLessonForStudent;

public class CompleteLessonForStudentCommandHandler : IRequestHandler<CompleteLessonForStudentCommand, CompleteLessonForStudentResult>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ILearningRepository _learningRepository;
    private readonly IMediator _mediator;
    private readonly IDomainEventService _domainEventService;
    private readonly ILogger<CompleteLessonForStudentCommandHandler> _logger;

    public CompleteLessonForStudentCommandHandler(
        IStudentRepository studentRepository,
        IEnrollmentRepository enrollmentRepository,
        ILearningRepository learningRepository,
        IMediator mediator,
        IDomainEventService domainEventService,
        ILogger<CompleteLessonForStudentCommandHandler> logger)
    {
        _studentRepository = studentRepository;
        _enrollmentRepository = enrollmentRepository;
        _learningRepository = learningRepository;
        _mediator = mediator;
        _domainEventService = domainEventService;
        _logger = logger;
    }

    public async Task<CompleteLessonForStudentResult> Handle(CompleteLessonForStudentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processando conclusão da lição {LessonId} para o estudante {StudentId}", 
            request.LessonId, request.StudentId);

        // Verificar se o estudante existe
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        if (student == null)
        {
            throw new NotFoundException($"Estudante com ID {request.StudentId} não encontrado");
        }

        // Obter informações da lição para determinar o curso
        var courseId = await GetCourseIdByLessonId(request.LessonId, cancellationToken);
        if (courseId == Guid.Empty)
        {
            throw new NotFoundException($"Lição com ID {request.LessonId} não encontrada");
        }

        // Verificar se o estudante está matriculado no curso
        var enrollments = await _enrollmentRepository.GetByStudentIdAsync(request.StudentId);
        var enrollment = enrollments.FirstOrDefault(e => e.CourseId == courseId && e.Status == StatusMatricula.Ativa);
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

        // Verificar se a lição já foi completada
        if (learningHistory.HasCompletedLesson(courseId, request.LessonId))
        {
            _logger.LogWarning("Lição {LessonId} já foi completada pelo estudante {StudentId}", 
                request.LessonId, request.StudentId);
            
            return new CompleteLessonForStudentResult
            {
                StudentId = request.StudentId,
                LessonId = request.LessonId,
                IsSuccessful = true,
                Score = request.Score,
                CourseProgress = await CalculateCourseProgress(courseId, learningHistory, cancellationToken),
                CourseCompleted = learningHistory.HasCompletedCourse(courseId)
            };
        }

        // Adicionar progresso da lição
        learningHistory.AddProgress(courseId, request.LessonId);

        // Calcular progresso do curso
        var courseProgress = await CalculateCourseProgress(courseId, learningHistory, cancellationToken);
        
        // Verificar se o curso foi completado
        var isCourseCompleted = await CheckIfCourseIsCompleted(courseId, learningHistory, cancellationToken);
        if (isCourseCompleted && !learningHistory.HasCompletedCourse(courseId))
        {
            learningHistory.CompleteCourse(courseId);
            
            // Disparar evento de conclusão do curso
            var courseCompletedEvent = new CourseCompletedEvent(
                request.StudentId, 
                courseId, 
                DateTime.UtcNow, 
                request.Score);
            
            await _domainEventService.PublishAsync(courseCompletedEvent);
            
            _logger.LogInformation("Curso {CourseId} completado pelo estudante {StudentId}", 
                courseId, request.StudentId);
        }

        // Salvar alterações
        await _learningRepository.SaveChangesAsync(cancellationToken);

        // Disparar evento de conclusão da lição
        var lessonCompletedEvent = new LessonCompletedEvent(
            request.StudentId, 
            courseId, 
            request.LessonId, 
            request.CompletionDate);
        
        await _domainEventService.PublishAsync(lessonCompletedEvent);

        _logger.LogInformation("Lição {LessonId} completada com sucesso pelo estudante {StudentId}", 
            request.LessonId, request.StudentId);

        return new CompleteLessonForStudentResult
        {
            StudentId = request.StudentId,
            LessonId = request.LessonId,
            IsSuccessful = true,
            Score = request.Score,
            CourseProgress = courseProgress,
            CourseCompleted = isCourseCompleted
        };
    }

    private async Task<Guid> GetCourseIdByLessonId(Guid lessonId, CancellationToken cancellationToken)
    {
        try
        {
            // Usar a query GetLessonById para obter informações da lição
            var lesson = await _mediator.Send(new GetLessonByIdQuery { LessonId = lessonId }, cancellationToken);
            return lesson?.CourseId ?? Guid.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter informações da lição {LessonId}", lessonId);
            return Guid.Empty;
        }
    }

    private async Task<int> CalculateCourseProgress(Guid courseId, LearningHistory learningHistory, CancellationToken cancellationToken)
    {
        try
        {
            var lessons = await _mediator.Send(new GetLessonsByCourseIdQuery { CourseId = courseId }, cancellationToken);
            var totalLessons = lessons.Count();
            var completedLessons = learningHistory.GetCompletedLessonsCount(courseId);
            
            return totalLessons > 0 ? (int)Math.Round((double)completedLessons / totalLessons * 100) : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular progresso do curso {CourseId}", courseId);
            return 0;
        }
    }

    private async Task<bool> CheckIfCourseIsCompleted(Guid courseId, LearningHistory learningHistory, CancellationToken cancellationToken)
    {
        try
        {
            var lessons = await _mediator.Send(new GetLessonsByCourseIdQuery { CourseId = courseId }, cancellationToken);
            var totalLessons = lessons.Count();
            var completedLessons = learningHistory.GetCompletedLessonsCount(courseId);
            
            return totalLessons > 0 && completedLessons >= totalLessons;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar se o curso {CourseId} foi completado", courseId);
            return false;
        }
    }
} 