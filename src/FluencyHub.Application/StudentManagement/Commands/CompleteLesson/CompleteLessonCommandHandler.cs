using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.ContentManagement.Queries.GetLessonById;
using FluencyHub.Application.StudentManagement.Queries.GetEnrollmentById;
using FluencyHub.StudentManagement.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
        IEnrollmentRepository enrollmentRepository,
        ILessonRepository lessonRepository,
        ILogger<CompleteLessonCommandHandler> logger)
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

        var enrollment = await _mediator.Send(new GetEnrollmentByIdQuery(request.EnrollmentId), cancellationToken) ?? throw new NotFoundException("Enrollment", request.EnrollmentId);
        if (enrollment.Status != EnrollmentStatus.Active.ToString())
        {
            throw new BadRequestException("Only active enrollments can be completed");
        }

        var lesson = await _mediator.Send(new GetLessonByIdQuery(request.LessonId), cancellationToken) ?? throw new NotFoundException("Lesson", request.LessonId);
        if (lesson.CourseId != enrollment.CourseId)
        {
            throw new BadRequestException("The lesson does not belong to the course of enrollment");
        }

        try
        {
            var learningHistory = await _learningRepository.GetByStudentIdAsync(enrollment.StudentId, cancellationToken);
            if (learningHistory == null)
            {
                learningHistory = new LearningHistory(enrollment.StudentId);
                await _learningRepository.AddAsync(learningHistory, cancellationToken);
            }

            if (await _learningRepository.HasCompletedLessonAsync(enrollment.StudentId, lesson.Id, cancellationToken))
            {
                if (!request.Completed)
                {
                    await _learningRepository.UncompleteLessonAsync(enrollment.StudentId, lesson.Id, cancellationToken);
                    return new CompleteLessonResult
                    {
                        IsCompleted = false,
                        Message = "Lesson has been marked as incomplete"
                    };
                }

                return new CompleteLessonResult
                {
                    IsCompleted = true,
                    Message = "Lesson was already completed"
                };
            }

            if (!request.Completed)
            {
                return new CompleteLessonResult
                {
                    IsCompleted = false,
                    Message = "Lesson was already not completed"
                };
            }

            await _learningRepository.CompleteLessonAsync(enrollment.StudentId, enrollment.CourseId, lesson.Id, cancellationToken);

            bool allLessonsCompleted = await IsAllLessonsCompletedAsync(enrollment.StudentId, enrollment.CourseId, cancellationToken);

            if (allLessonsCompleted)
            {
                learningHistory.CompleteCourse(enrollment.CourseId);

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
                    Message = "Lesson completed successfully and course has been completed automatically",
                    CourseCompleted = true,
                    IsCompleted = true
                };
            }

            return new CompleteLessonResult
            {
                EnrollmentId = request.EnrollmentId,
                LessonId = request.LessonId,
                Message = "Lesson completed successfully",
                CourseCompleted = false,
                IsCompleted = true
            };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new BadRequestException("Concurrency error when saving lesson progress. Please try again.", ex);
        }
        catch (Exception ex)
        {
            throw new BadRequestException($"Error completing lesson: {ex.Message}", ex);
        }
    }

    private async Task<bool> IsAllLessonsCompletedAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdWithLessonsAsync(courseId, cancellationToken);
        if (course == null || !course.Lessons.Any())
        {
            return false;
        }

        var completedLessonIds = await _learningRepository.GetCompletedLessonIdsAsync(studentId, courseId, cancellationToken);
        var allLessonIds = course.Lessons.Select(l => l.Id);

        return !allLessonIds.Except(completedLessonIds).Any();
    }
}