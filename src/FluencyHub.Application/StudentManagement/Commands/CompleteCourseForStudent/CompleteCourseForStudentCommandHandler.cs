using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.ContentManagement.Queries.GetCourseById;
using FluencyHub.Application.StudentManagement.Queries.GetStudentById;
using FluencyHub.StudentManagement.Domain;
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
        var student = await _mediator.Send(new GetStudentByIdQuery(request.StudentId), cancellationToken) ?? throw new NotFoundException("Student", request.StudentId);

        var course = await _mediator.Send(new GetCourseByIdQuery(request.CourseId), cancellationToken) ?? throw new NotFoundException("Course", request.CourseId);

        var learningHistory = await _learningRepository.GetByStudentIdAsync(request.StudentId, cancellationToken);
        if (learningHistory == null)
        {
            learningHistory = new LearningHistory(request.StudentId);
            await _learningRepository.AddAsync(learningHistory, cancellationToken);
        }

        if (learningHistory.HasCompletedCourse(request.CourseId))
        {
            return new CompleteCourseForStudentResult
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                Message = "Course has already been completed",
                Success = true,
                CompletedLessons = await _learningRepository.GetCompletedLessonsCountAsync(request.StudentId, request.CourseId, cancellationToken),
                TotalLessons = await _courseRepository.GetLessonsCountByCourseId(request.CourseId, cancellationToken)
            };
        }

        var allLessonIds = course.Lessons.Select(l => l.Id).ToList();
        var totalLessons = allLessonIds.Count;

        var completedLessonIds = (await _learningRepository.GetCompletedLessonIdsAsync(request.StudentId, request.CourseId, cancellationToken)).ToList();
        var completedLessonsCount = completedLessonIds.Count;

        var notCompletedLessonIds = allLessonIds.Where(id => !completedLessonIds.Contains(id)).ToList();

        if (notCompletedLessonIds.Count != 0)
        {
            return new CompleteCourseForStudentResult
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                Message = $"All lessons must be completed before finishing the course. Completed lessons: {completedLessonsCount}/{totalLessons}. Missing: {notCompletedLessonIds.Count} lessons.",
                Success = false,
                CompletedLessons = completedLessonsCount,
                TotalLessons = totalLessons
            };
        }

        learningHistory.CompleteCourse(request.CourseId);
        await _learningRepository.SaveChangesAsync(cancellationToken);

        return new CompleteCourseForStudentResult
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            Message = "Course marked as completed successfully",
            Success = true,
            CompletedLessons = completedLessonsCount,
            TotalLessons = totalLessons
        };
    }
}