using FluencyHub.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;
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

    public async Task<bool> HasCompletedLessonAsync(Guid studentId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        return await _context.CompletedLessons
            .AnyAsync(cl => cl.CourseProgress.LearningHistoryId == studentId && cl.LessonId == lessonId, cancellationToken);
    }

    public async Task<int> GetCompletedLessonsCountAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default)
    {
        var courseProgress = await GetCourseProgressAsync(studentId, courseId, cancellationToken);
        return courseProgress?.CompletedLessons.Count ?? 0;
    }

    public async Task<IEnumerable<Guid>> GetCompletedLessonIdsAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default)
    {
        var courseProgress = await GetCourseProgressAsync(studentId, courseId, cancellationToken);
        return courseProgress?.CompletedLessons.Select(cl => cl.LessonId).ToList() ?? [];
    }

    public async Task AddAsync(LearningHistory learningHistory, CancellationToken cancellationToken = default)
    {
        await _context.LearningHistories.AddAsync(learningHistory, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteLessonAsync(Guid studentId, Guid courseId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        var courseProgress = await GetCourseProgressAsync(studentId, courseId, cancellationToken);
        if (courseProgress == null)
        {
            var learningHistory = await GetByStudentIdAsync(studentId, cancellationToken);
            if (learningHistory == null)
            {
                learningHistory = new LearningHistory(studentId);
                await _context.LearningHistories.AddAsync(learningHistory, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            courseProgress = new CourseProgress(courseId)
            {
                LearningHistoryId = studentId
            };
            await _context.CourseProgresses.AddAsync(courseProgress, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        bool isCompleted = await _context.CompletedLessons
            .AnyAsync(cl => cl.CourseProgressId == courseProgress.Id && cl.LessonId == lessonId, cancellationToken);

        if (!isCompleted)
        {
            var completedLesson = new CompletedLesson
            {
                LessonId = lessonId,
                CourseProgressId = courseProgress.Id,
                CompletedAt = DateTime.UtcNow,
                CourseProgress = courseProgress
            };
            await _context.CompletedLessons.AddAsync(completedLesson, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UncompleteLessonAsync(Guid studentId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        var completedLesson = await _context.CompletedLessons
            .FirstOrDefaultAsync(cl => cl.CourseProgress.LearningHistoryId == studentId && cl.LessonId == lessonId, cancellationToken);

        if (completedLesson != null)
        {
            _context.CompletedLessons.Remove(completedLesson);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateAsync(LearningHistory learningHistory, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.ChangeTracker.Clear();

            var existingHistory = await _context.LearningHistories
                .FirstOrDefaultAsync(lh => lh.Id == learningHistory.Id, cancellationToken);

            if (existingHistory == null)
            {
                await _context.LearningHistories.AddAsync(learningHistory, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return;
            }

            existingHistory.UpdatedAt = DateTime.UtcNow;

            foreach (var progress in learningHistory.CourseProgress)
            {
                var existingProgress = await _context.CourseProgresses
                    .FirstOrDefaultAsync(cp => cp.CourseId == progress.CourseId &&
                                               cp.LearningHistoryId == learningHistory.Id,
                                         cancellationToken);

                if (existingProgress == null)
                {
                    existingProgress = new CourseProgress(progress.CourseId)
                    {
                        LearningHistoryId = learningHistory.Id
                    };
                    await _context.CourseProgresses.AddAsync(existingProgress, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                if (progress.IsCompleted && !existingProgress.IsCompleted)
                {
                    existingProgress.CompleteCourse();
                }

                foreach (var lessonToAdd in progress.CompletedLessons)
                {
                    bool lessonAlreadyCompleted = await _context.CompletedLessons
                        .AnyAsync(cl => cl.CourseProgressId == existingProgress.Id &&
                                       cl.LessonId == lessonToAdd.LessonId,
                                 cancellationToken);

                    if (!lessonAlreadyCompleted)
                    {
                        var completedLesson = new CompletedLesson
                        {
                            LessonId = lessonToAdd.LessonId,
                            CourseProgressId = existingProgress.Id,
                            CompletedAt = DateTime.UtcNow,
                            CourseProgress = existingProgress
                        };

                        await _context.CompletedLessons.AddAsync(completedLesson, cancellationToken);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            foreach (var progress in learningHistory.CourseProgress)
            {
                foreach (var lesson in progress.CompletedLessons)
                {
                    try
                    {
                        bool exists = await _context.CompletedLessons
                            .AnyAsync(cl => cl.LessonId == lesson.LessonId &&
                                         cl.CourseProgress.CourseId == progress.CourseId &&
                                         cl.CourseProgress.LearningHistoryId == learningHistory.Id,
                                  cancellationToken);

                        if (!exists)
                        {
                            var courseProgress = await _context.CourseProgresses
                                .FirstOrDefaultAsync(cp => cp.CourseId == progress.CourseId &&
                                                      cp.LearningHistoryId == learningHistory.Id,
                                               cancellationToken);

                            if (courseProgress != null)
                            {
                                await _context.Database.ExecuteSqlRawAsync(
                                    "INSERT INTO CompletedLessons (Id, LessonId, CourseProgressId, CompletedAt) VALUES ({0}, {1}, {2}, {3})",
                                    Guid.NewGuid(), lesson.LessonId, courseProgress.Id, DateTime.UtcNow);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}