using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;

public class LearningRepository : ILearningRepository
{
    private readonly StudentDbContext _dbContext;
    private readonly FluencyHub.SharedKernel.Events.IDomainEventService _eventService;

    public LearningRepository(StudentDbContext dbContext, FluencyHub.SharedKernel.Events.IDomainEventService eventService)
    {
        _dbContext = dbContext;
        _eventService = eventService;
    }

    public async Task<LearningHistory?> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LearningHistories
            .Include(lh => lh.CourseProgresses)
                .ThenInclude(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(lh => lh.StudentId == studentId, cancellationToken);
    }

    public async Task<CourseProgress?> GetCourseProgressAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default)
    {
        var learningHistory = await GetByStudentIdAsync(studentId, cancellationToken);
        
        return learningHistory?.CourseProgresses
            .FirstOrDefault(cp => cp.CourseId == courseId);
    }

    public async Task<bool> HasCompletedLessonAsync(Guid studentId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        // Buscar o histórico de aprendizado primeiro
        var learningHistory = await GetByStudentIdAsync(studentId, cancellationToken);
        if (learningHistory == null)
            return false;
            
        // Verificar em todos os progressos de curso se a lição foi completada
        return learningHistory.CourseProgresses
            .SelectMany(cp => cp.CompletedLessons)
            .Any(cl => cl.LessonId == lessonId);
    }

    public async Task<int> GetCompletedLessonsCountAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default)
    {
        var courseProgress = await GetCourseProgressAsync(studentId, courseId, cancellationToken);
        return courseProgress?.CompletedLessons.Count ?? 0;
    }

    public async Task<IEnumerable<Guid>> GetCompletedLessonIdsAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default)
    {
        var courseProgress = await GetCourseProgressAsync(studentId, courseId, cancellationToken);
        return courseProgress?.CompletedLessons.Select(cl => cl.LessonId).ToList() ?? new List<Guid>();
    }

    public async Task AddAsync(LearningHistory learningHistory, CancellationToken cancellationToken = default)
    {
        await _dbContext.LearningHistories.AddAsync(learningHistory, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteLessonAsync(Guid studentId, Guid courseId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        // Buscar o histórico de aprendizado primeiro
        var learningHistory = await GetByStudentIdAsync(studentId, cancellationToken);
        
        if (learningHistory == null)
        {
            learningHistory = new LearningHistory(studentId);
            await _dbContext.LearningHistories.AddAsync(learningHistory, cancellationToken);
        }

        var courseProgress = learningHistory.CourseProgresses
            .FirstOrDefault(cp => cp.CourseId == courseId);

        if (courseProgress == null)
        {
            courseProgress = new CourseProgress(courseId);
            learningHistory.AddCourseProgress(courseProgress);
        }

        courseProgress.CompleteLesson(lessonId);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UncompleteLessonAsync(Guid studentId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        var learningHistory = await GetByStudentIdAsync(studentId, cancellationToken);
        if (learningHistory == null)
            return;
            
        var completedLesson = learningHistory.CourseProgresses
            .SelectMany(cp => cp.CompletedLessons)
            .FirstOrDefault(cl => cl.LessonId == lessonId);
            
        if (completedLesson != null)
        {
            _dbContext.CompletedLessons.Remove(completedLesson);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateAsync(LearningHistory learningHistory, CancellationToken cancellationToken = default)
    {
        _dbContext.LearningHistories.Update(learningHistory);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<LearningHistory> GetLearningHistoryByStudentIdAsync(Guid studentId)
    {
        return await _dbContext.LearningHistories
            .Include(lh => lh.CourseProgresses)
            .FirstOrDefaultAsync(lh => lh.StudentId == studentId);
    }

    public async Task<CourseProgress> GetCourseProgressAsync(Guid courseId, Guid learningHistoryId)
    {
        return await _dbContext.CourseProgresses
            .Include(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(cp => cp.CourseId == courseId && cp.LearningHistoryId == learningHistoryId);
    }

    public async Task<CourseProgress> GetCourseProgressByIdAsync(Guid courseProgressId)
    {
        return await _dbContext.CourseProgresses
            .Include(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(cp => cp.Id == courseProgressId);
    }

    public async Task<IEnumerable<CourseProgress>> GetCourseProgressesByStudentIdAsync(Guid studentId)
    {
        var learningHistory = await _dbContext.LearningHistories
            .Include(lh => lh.CourseProgresses)
            .ThenInclude(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(lh => lh.StudentId == studentId);

        return learningHistory?.CourseProgresses ?? new List<CourseProgress>();
    }

    public async Task<IEnumerable<CompletedLesson>> GetCompletedLessonsByCourseProgressIdAsync(Guid courseProgressId)
    {
        var courseProgress = await _dbContext.CourseProgresses
            .Include(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(cp => cp.Id == courseProgressId);

        return courseProgress?.CompletedLessons ?? new List<CompletedLesson>();
    }

    public async Task AddLearningHistoryAsync(LearningHistory learningHistory)
    {
        await _dbContext.LearningHistories.AddAsync(learningHistory);
    }

    public async Task AddCourseProgressAsync(CourseProgress courseProgress)
    {
        await _dbContext.CourseProgresses.AddAsync(courseProgress);
    }

    public async Task AddCompletedLessonAsync(CompletedLesson completedLesson)
    {
        await _dbContext.CompletedLessons.AddAsync(completedLesson);
    }
} 