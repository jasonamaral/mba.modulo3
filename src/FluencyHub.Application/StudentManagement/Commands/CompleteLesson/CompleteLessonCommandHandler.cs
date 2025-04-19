using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.ContentManagement.Queries.GetCourseById;
using FluencyHub.Application.ContentManagement.Queries.GetLessonById;
using FluencyHub.Application.StudentManagement.Queries.GetEnrollmentById;
using FluencyHub.Domain.StudentManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FluencyHub.Application.StudentManagement.Commands.CompleteLesson;

public class CompleteLessonCommandHandler : IRequestHandler<CompleteLessonCommand, CompleteLessonResult>
{
    private readonly IMediator _mediator;
    private readonly ILearningRepository _learningRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public CompleteLessonCommandHandler(
        IMediator mediator,
        ILearningRepository learningRepository,
        ICourseRepository courseRepository,
        IEnrollmentRepository enrollmentRepository)
    {
        _mediator = mediator;
        _learningRepository = learningRepository;
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<CompleteLessonResult> Handle(CompleteLessonCommand request, CancellationToken cancellationToken)
    {
        if (!request.Completed)
        {
            throw new BadRequestException("Request must contain 'completed: true' value");
        }

        // Verificar se a matrícula existe e está ativa
        var enrollment = await _mediator.Send(new GetEnrollmentByIdQuery(request.EnrollmentId), cancellationToken);
        if (enrollment == null)
        {
            throw new NotFoundException("Enrollment", request.EnrollmentId);
        }

        if (enrollment.Status != EnrollmentStatus.Active.ToString())
        {
            throw new BadRequestException("Only active enrollments can be completed");
        }

        // Verificar se a lição existe e pertence ao curso da matrícula
        var lesson = await _mediator.Send(new GetLessonByIdQuery(request.LessonId), cancellationToken);
        if (lesson == null)
        {
            throw new NotFoundException("Lesson", request.LessonId);
        }

        if (lesson.CourseId != enrollment.CourseId)
        {
            throw new BadRequestException("The lesson does not belong to the course of enrollment");
        }

        try
        {
            // Buscar ou criar o histórico de aprendizado
            var learningHistory = await _learningRepository.GetByStudentIdAsync(enrollment.StudentId, cancellationToken);
            
            // Se não existir, cria um novo
            if (learningHistory == null)
            {
                learningHistory = new LearningHistory(enrollment.StudentId);
            }
            
            // Verificar se a lição já foi concluída
            bool lessonAlreadyCompleted = learningHistory.HasCompletedLesson(enrollment.CourseId, request.LessonId);
            if (lessonAlreadyCompleted)
            {
                return new CompleteLessonResult
                {
                    EnrollmentId = request.EnrollmentId,
                    LessonId = request.LessonId,
                    Completed = request.Completed,
                    Message = "Lesson was already completed",
                    CourseCompleted = learningHistory.HasCompletedCourse(enrollment.CourseId)
                };
            }

            // Adicionar a lição concluída
            learningHistory.AddProgress(enrollment.CourseId, request.LessonId);
            
            // Salvar as alterações
            await _learningRepository.UpdateAsync(learningHistory, cancellationToken);

            // Verificar se todas as lições do curso foram concluídas
            var course = await _mediator.Send(new GetCourseByIdQuery(enrollment.CourseId), cancellationToken);
            
            // Recarregar o histórico para ter certeza que temos a versão mais atualizada
            learningHistory = await _learningRepository.GetByStudentIdAsync(enrollment.StudentId, cancellationToken);
            
            if (course != null && learningHistory != null)
            {
                // Obter todas as lições do curso
                var allLessonIds = course.Lessons.Select(l => l.Id).ToList();

                // Verificar se todas as lições foram concluídas
                bool allLessonsCompleted = allLessonIds.All(id => 
                    learningHistory.HasCompletedLesson(enrollment.CourseId, id));

                // Se todas as lições estiverem concluídas, concluir o curso
                if (allLessonsCompleted && !learningHistory.HasCompletedCourse(enrollment.CourseId))
                {
                    learningHistory.CompleteCourse(enrollment.CourseId);
                    await _learningRepository.UpdateAsync(learningHistory, cancellationToken);
                    
                    // Atualizar o status da matrícula
                    var enrollmentEntity = await _enrollmentRepository.GetByIdAsync(request.EnrollmentId);
                    if (enrollmentEntity != null)
                    {
                        enrollmentEntity.CompleteEnrollment();
                        await _enrollmentRepository.SaveChangesAsync(cancellationToken);
                    }

                    return new CompleteLessonResult
                    {
                        EnrollmentId = request.EnrollmentId,
                        LessonId = request.LessonId,
                        Completed = request.Completed,
                        Message = "Lesson completed successfully and course has been completed automatically",
                        CourseCompleted = true
                    };
                }
            }

            // Retornar o resultado
            return new CompleteLessonResult
            {
                EnrollmentId = request.EnrollmentId,
                LessonId = request.LessonId,
                Completed = request.Completed,
                Message = "Lesson completed successfully",
                CourseCompleted = false
            };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Propaga a exceção para que o controlador possa tratá-la
            throw new BadRequestException("Erro de concorrência ao salvar o progresso da lição. Por favor, tente novamente.", ex);
        }
        catch (Exception ex)
        {
            throw new BadRequestException($"Erro ao completar lição: {ex.Message}", ex);
        }
    }
} 