using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.ContentManagement.Queries.GetCourseById;
using FluencyHub.Application.ContentManagement.Queries.GetLessonById;
using FluencyHub.Application.StudentManagement.Queries.GetStudentById;
using FluencyHub.Domain.StudentManagement;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.CompleteLessonForStudent;

public class CompleteLessonForStudentCommandHandler : IRequestHandler<CompleteLessonForStudentCommand, CompleteLessonForStudentResult>
{
    private readonly IMediator _mediator;
    private readonly ILearningRepository _learningRepository;
    private readonly ICourseRepository _courseRepository;

    public CompleteLessonForStudentCommandHandler(
        IMediator mediator,
        ILearningRepository learningRepository,
        ICourseRepository courseRepository)
    {
        _mediator = mediator;
        _learningRepository = learningRepository;
        _courseRepository = courseRepository;
    }

    public async Task<CompleteLessonForStudentResult> Handle(CompleteLessonForStudentCommand request, CancellationToken cancellationToken)
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

        // Verificar se a lição existe e pertence ao curso
        var lesson = await _mediator.Send(new GetLessonByIdQuery(request.LessonId), cancellationToken);
        if (lesson == null)
        {
            throw new NotFoundException("Lesson", request.LessonId);
        }

        if (lesson.CourseId != request.CourseId)
        {
            throw new BadRequestException("A lição não pertence ao curso especificado");
        }

        // Obter ou criar o histórico de aprendizado
        var learningHistory = await _learningRepository.GetByStudentIdAsync(request.StudentId, cancellationToken);
        if (learningHistory == null)
        {
            learningHistory = new LearningHistory(request.StudentId);
            await _learningRepository.AddAsync(learningHistory, cancellationToken);
        }

        // Verificar se a lição já foi concluída
        if (learningHistory.HasCompletedLesson(request.CourseId, request.LessonId))
        {
            return new CompleteLessonForStudentResult
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                LessonId = request.LessonId,
                Message = "Lição já foi concluída anteriormente",
                Success = true
            };
        }

        // Adicionar o progresso
        learningHistory.AddProgress(request.CourseId, request.LessonId);
        await _learningRepository.SaveChangesAsync(cancellationToken);

        return new CompleteLessonForStudentResult
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            LessonId = request.LessonId,
            Message = "Lição marcada como concluída com sucesso",
            Success = true
        };
    }
} 