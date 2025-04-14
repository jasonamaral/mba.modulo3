using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.StudentManagement;
using Microsoft.EntityFrameworkCore;

namespace FluencyHub.Infrastructure.Persistence.Repositories;

public class LearningRepository : ILearningRepository
{
    private readonly FluencyHubDbContext _context;

    public LearningRepository(FluencyHubDbContext context)
    {
        _context = context;
    }

    public async Task<LearningHistory?> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.LearningHistories
            .Include(lh => lh.CourseProgress)
                .ThenInclude(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(lh => lh.Id == studentId, cancellationToken);
    }

    public async Task<CourseProgress?> GetCourseProgressAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _context.CourseProgresses
            .Include(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(cp => cp.LearningHistoryId == studentId && cp.CourseId == courseId, cancellationToken);
    }

    public async Task<bool> HasCompletedLessonAsync(Guid studentId, Guid courseId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        var courseProgress = await GetCourseProgressAsync(studentId, courseId, cancellationToken);
        return courseProgress != null && courseProgress.CompletedLessons.Any(cl => cl.LessonId == lessonId);
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
        await _context.LearningHistories.AddAsync(learningHistory, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(LearningHistory learningHistory, CancellationToken cancellationToken = default)
    {
        _context.LearningHistories.Update(learningHistory);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
} 