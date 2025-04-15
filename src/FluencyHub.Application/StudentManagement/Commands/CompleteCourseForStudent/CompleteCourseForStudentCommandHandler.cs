using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.ContentManagement.Queries.GetCourseById;
using FluencyHub.Application.StudentManagement.Queries.GetStudentById;
using FluencyHub.Domain.StudentManagement;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.CompleteCourseForStudent;

public class CompleteCourseForStudentCommandHandler : IRequestHandler<CompleteCourseForStudentCommand, CompleteCourseForStudentResult>
{
    private readonly IMediator _mediator;
    private readonly ILearningRepository _learningRepository;
    private readonly ICourseRepository _courseRepository;

    public CompleteCourseForStudentCommandHandler(
        IMediator mediator,
        ILearningRepository learningRepository,
        ICourseRepository courseRepository)
    {
        _mediator = mediator;
        _learningRepository = learningRepository;
        _courseRepository = courseRepository;
    }

    public async Task<CompleteCourseForStudentResult> Handle(CompleteCourseForStudentCommand request, CancellationToken cancellationToken)
    {
        // Verificar se o estudante existe
        var student = await _mediator.Send(new GetStudentByIdQuery(request.StudentId), cancellationToken);
        if (student == null)
        {
            throw new NotFoundException("Student", request.StudentId);
        }

        // Verificar se o curso existe
        var course = await _mediator.Send(new GetCourseByIdQuery(request.CourseId), cancellationToken);
        if (course == null)
        {
            throw new NotFoundException("Course", request.CourseId);
        }

        // Obter ou criar o histórico de aprendizado
        var learningHistory = await _learningRepository.GetByStudentIdAsync(request.StudentId, cancellationToken);
        if (learningHistory == null)
        {
            learningHistory = new LearningHistory(request.StudentId);
            await _learningRepository.AddAsync(learningHistory, cancellationToken);
        }

        // Verificar se o curso já está concluído
        if (learningHistory.HasCompletedCourse(request.CourseId))
        {
            return new CompleteCourseForStudentResult
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                Message = "Curso já foi concluído anteriormente",
                Success = true,
                CompletedLessons = await _learningRepository.GetCompletedLessonsCountAsync(request.StudentId, request.CourseId, cancellationToken),
                TotalLessons = await _courseRepository.GetLessonsCountByCourseId(request.CourseId, cancellationToken)
            };
        }

        // Obter todas as lições do curso
        var allLessonIds = course.Lessons.Select(l => l.Id).ToList();
        var totalLessons = allLessonIds.Count;

        // Obter as lições concluídas
        var completedLessonIds = (await _learningRepository.GetCompletedLessonIdsAsync(request.StudentId, request.CourseId, cancellationToken)).ToList();
        var completedLessonsCount = completedLessonIds.Count;

        // Verificar quais lições ainda não foram concluídas
        var notCompletedLessonIds = allLessonIds.Where(id => !completedLessonIds.Contains(id)).ToList();

        // Verificar se todas as lições foram concluídas
        if (notCompletedLessonIds.Any())
        {
            return new CompleteCourseForStudentResult
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                Message = $"Todas as lições devem ser concluídas antes de finalizar o curso. Lições concluídas: {completedLessonsCount}/{totalLessons}. Faltam: {notCompletedLessonIds.Count} lições.",
                Success = false,
                CompletedLessons = completedLessonsCount,
                TotalLessons = totalLessons
            };
        }

        // Marcar o curso como concluído
        learningHistory.CompleteCourse(request.CourseId);
        await _learningRepository.SaveChangesAsync(cancellationToken);

        return new CompleteCourseForStudentResult
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            Message = "Curso marcado como concluído com sucesso",
            Success = true,
            CompletedLessons = completedLessonsCount,
            TotalLessons = totalLessons
        };
    }
} 