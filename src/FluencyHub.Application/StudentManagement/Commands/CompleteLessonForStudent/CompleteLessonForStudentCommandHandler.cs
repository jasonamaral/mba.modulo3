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
        var student = await _mediator.Send(new GetStudentByIdQuery(request.StudentId), cancellationToken) ?? throw new NotFoundException("Student", request.StudentId);

        var course = await _mediator.Send(new GetCourseByIdQuery(request.CourseId), cancellationToken) ?? throw new NotFoundException("Course", request.CourseId);

        var lesson = await _mediator.Send(new GetLessonByIdQuery(request.LessonId), cancellationToken) ?? throw new NotFoundException("Lesson", request.LessonId);
        if (lesson.CourseId != request.CourseId)
        {
            throw new BadRequestException("The lesson does not belong to the specified course");
        }

        var learningHistory = await _learningRepository.GetByStudentIdAsync(request.StudentId, cancellationToken);
        if (learningHistory == null)
        {
            learningHistory = new LearningHistory(request.StudentId);
            await _learningRepository.AddAsync(learningHistory, cancellationToken);
        }

        if (learningHistory.HasCompletedLesson(request.CourseId, request.LessonId))
        {
            return new CompleteLessonForStudentResult
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                LessonId = request.LessonId,
                Message = "Lesson has already been completed previously",
                Success = true
            };
        }

        learningHistory.AddProgress(request.CourseId, request.LessonId);
        await _learningRepository.SaveChangesAsync(cancellationToken);

        return new CompleteLessonForStudentResult
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            LessonId = request.LessonId,
            Message = "Lesson marked as completed successfully",
            Success = true
        };
    }
}