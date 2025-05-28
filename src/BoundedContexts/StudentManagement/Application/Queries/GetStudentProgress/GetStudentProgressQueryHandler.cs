using MediatR;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Application.Common.Exceptions;
using FluencyHub.SharedKernel.Queries;
using FluencyHub.ContentManagement.Application.Queries.GetLessonsByCourseId;
using Microsoft.Extensions.Logging;
using IStudentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;
using IEnrollmentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IEnrollmentRepository;

namespace FluencyHub.StudentManagement.Application.Queries.GetStudentProgress;

public class GetStudentProgressQueryHandler : IRequestHandler<GetStudentProgressQuery, StudentProgressViewModel>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ILearningRepository _learningRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<GetStudentProgressQueryHandler> _logger;

    public GetStudentProgressQueryHandler(
        IStudentRepository studentRepository,
        IEnrollmentRepository enrollmentRepository,
        ILearningRepository learningRepository,
        IMediator mediator,
        ILogger<GetStudentProgressQueryHandler> logger)
    {
        _studentRepository = studentRepository;
        _enrollmentRepository = enrollmentRepository;
        _learningRepository = learningRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<StudentProgressViewModel> Handle(GetStudentProgressQuery request, CancellationToken cancellationToken)
    {

        var student = await _studentRepository.GetByIdAsync(request.StudentId) ?? throw new NotFoundException($"Student with ID {request.StudentId} not found");

        var enrollments = await _enrollmentRepository.GetByStudentIdAsync(request.StudentId);
        
        if (!enrollments.Any())
        {
            return new StudentProgressViewModel
            {
                StudentId = request.StudentId,
                StudentName = student.FullName,
                TotalCourses = 0,
                CompletedCourses = 0,
                TotalLessonsAcrossAllCourses = 0,
                CompletedLessonsAcrossAllCourses = 0,
                OverallProgressPercentage = 0,
                LastActivityDate = null,
                CourseProgresses = []
            };
        }

        var courseProgresses = await _learningRepository.GetCourseProgressesByStudentIdAsync(request.StudentId);
        
        var courseProgressDtos = new List<CourseProgressDto>();
        var totalLessonsAcrossAllCourses = 0;
        var completedLessonsAcrossAllCourses = 0;
        var completedCourses = 0;
        DateTime? lastActivityDate = null;

        foreach (var enrollment in enrollments)
        {
            try
            {

                var courseInfo = await _mediator.Send(new GetCourseById { CourseId = enrollment.CourseId }, cancellationToken);
                if (courseInfo == null)
                {
                    continue;
                }

                var courseProgress = courseProgresses.FirstOrDefault(cp => cp.CourseId == enrollment.CourseId);

                var totalLessons = await GetTotalLessonsForCourse(enrollment.CourseId, cancellationToken);
                
                var completedLessons = courseProgress?.GetCompletedLessonsCount() ?? 0;
                var progressPercentage = totalLessons > 0 ? (int)Math.Round((double)completedLessons / totalLessons * 100) : 0;

                var lessonProgress = await GetLessonProgressDetails(enrollment.CourseId, courseProgress, cancellationToken);

                var isCourseCompleted = courseProgress?.IsCompleted ?? false;
                if (isCourseCompleted)
                {
                    completedCourses++;
                }

                if (courseProgress?.LastUpdated != null && 
                    (lastActivityDate == null || courseProgress.LastUpdated > lastActivityDate))
                {
                    lastActivityDate = courseProgress.LastUpdated;
                }

                totalLessonsAcrossAllCourses += totalLessons;
                completedLessonsAcrossAllCourses += completedLessons;

                courseProgressDtos.Add(new CourseProgressDto
                {
                    CourseId = enrollment.CourseId,
                    CourseName = courseInfo.Name,
                    TotalLessons = totalLessons,
                    CompletedLessons = completedLessons,
                    ProgressPercentage = progressPercentage,
                    EnrollmentDate = enrollment.EnrollmentDate,
                    LastActivityDate = courseProgress?.LastUpdated,
                    IsCompleted = isCourseCompleted,
                    CompletionDate = enrollment.CompletionDate,
                    LessonProgress = lessonProgress
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Curso {CourseId}. Estudante {StudentId}", enrollment.CourseId, request.StudentId);
            }
        }

        var overallProgressPercentage = totalLessonsAcrossAllCourses > 0 
            ? (int)Math.Round((double)completedLessonsAcrossAllCourses / totalLessonsAcrossAllCourses * 100) 
            : 0;

        var result = new StudentProgressViewModel
        {
            StudentId = request.StudentId,
            StudentName = student.FullName,
            TotalCourses = enrollments.Count(),
            CompletedCourses = completedCourses,
            TotalLessonsAcrossAllCourses = totalLessonsAcrossAllCourses,
            CompletedLessonsAcrossAllCourses = completedLessonsAcrossAllCourses,
            OverallProgressPercentage = overallProgressPercentage,
            LastActivityDate = lastActivityDate,
            CourseProgresses = courseProgressDtos
        };

        return result;
    }

    private async Task<int> GetTotalLessonsForCourse(Guid courseId, CancellationToken cancellationToken)
    {
        try
        {
            var lessons = await _mediator.Send(new GetLessonsByCourseIdQuery { CourseId = courseId }, cancellationToken);
            return lessons.Count();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Curso {CourseId}", courseId);
            return 0;
        }
    }

    private async Task<List<LessonProgressDto>> GetLessonProgressDetails(Guid courseId, Domain.CourseProgress? courseProgress, CancellationToken cancellationToken)
    {
        try
        {
            var lessonProgress = new List<LessonProgressDto>();

            var lessons = await _mediator.Send(new GetLessonsByCourseIdQuery { CourseId = courseId }, cancellationToken);
            
            foreach (var lesson in lessons.OrderBy(l => l.Order))
            {
                var isCompleted = courseProgress?.HasCompletedLesson(lesson.Id) ?? false;
                var completionDate = isCompleted 
                    ? courseProgress?.CompletedLessons.FirstOrDefault(cl => cl.LessonId == lesson.Id)?.CompletedAt
                    : null;

                lessonProgress.Add(new LessonProgressDto
                {
                    LessonId = lesson.Id,
                    Title = lesson.Title,
                    Order = lesson.Order,
                    IsCompleted = isCompleted,
                    CompletionDate = completionDate,
                    Score = null
                });
            }

            return lessonProgress;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Curso {CourseId}", courseId);
            return [];
        }
    }
} 