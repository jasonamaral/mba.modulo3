using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;

public class LearningRepository : ILearningRepository
{
    private readonly StudentDbContext _context;

    public LearningRepository(StudentDbContext context)
    {
        _context = context;
    }

    public async Task<LearningHistory?> GetByStudentIdAsync(Guid studentId)
    {
        return await _context.LearningHistories
            .Include(lh => lh.CourseProgresses)
            .ThenInclude(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(lh => lh.StudentId == studentId);
    }

    public async Task<CourseProgress> GetCourseProgressAsync(Guid courseId, Guid learningHistoryId)
    {
        var progress = await _context.CourseProgresses
            .Include(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(cp => cp.CourseId == courseId && cp.LearningHistoryId == learningHistoryId);
            
        if (progress == null)
            throw new InvalidOperationException($"CourseProgress not found for courseId {courseId} and learningHistoryId {learningHistoryId}");
            
        return progress;
    }

    public async Task<IEnumerable<CourseProgress>> GetAllCourseProgressAsync(Guid studentId)
    {
        var learningHistory = await GetByStudentIdAsync(studentId);
        if (learningHistory == null)
            return new List<CourseProgress>();

        return learningHistory.CourseProgresses;
    }

    public async Task AddLearningHistoryAsync(LearningHistory learningHistory)
    {
        await _context.LearningHistories.AddAsync(learningHistory);
    }

    public async Task AddCourseProgressAsync(CourseProgress courseProgress)
    {
        await _context.CourseProgresses.AddAsync(courseProgress);
    }

    public async Task AddCompletedLessonAsync(CompletedLesson completedLesson)
    {
        await _context.CompletedLessons.AddAsync(completedLesson);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasCompletedLessonAsync(Guid studentId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        var learningHistory = await GetByStudentIdAsync(studentId);
        if (learningHistory == null)
            return false;
            
        return learningHistory.CourseProgresses
            .SelectMany(cp => cp.CompletedLessons)
            .Any(cl => cl.LessonId == lessonId);
    }

    public async Task<int> GetCompletedLessonsCountAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default)
    {
        var courseProgress = await GetCourseProgressAsync(courseId, studentId);
        return courseProgress?.CompletedLessons.Count ?? 0;
    }

    public async Task<IEnumerable<Guid>> GetCompletedLessonIdsAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default)
    {
        var courseProgress = await GetCourseProgressAsync(courseId, studentId);
        return courseProgress?.CompletedLessons.Select(cl => cl.LessonId).ToList() ?? new List<Guid>();
    }

    public async Task CompleteLessonAsync(Guid studentId, Guid courseId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        var learningHistory = await GetByStudentIdAsync(studentId);
        
        if (learningHistory == null)
        {
            learningHistory = new LearningHistory(studentId);
            await _context.LearningHistories.AddAsync(learningHistory, cancellationToken);
        }

        var courseProgress = learningHistory.CourseProgresses
            .FirstOrDefault(cp => cp.CourseId == courseId);

        if (courseProgress == null)
        {
            courseProgress = new CourseProgress(courseId)
            {
                LearningHistoryId = learningHistory.Id
            };
            learningHistory.AddCourseProgress(courseProgress);
        }

        courseProgress.CompleteLesson(lessonId);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UncompleteLessonAsync(Guid studentId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        var learningHistory = await GetByStudentIdAsync(studentId);
        if (learningHistory == null)
            return;
            
        var completedLesson = await _context.CompletedLessons
            .FirstOrDefaultAsync(cl => cl.LessonId == lessonId && 
                cl.CourseProgress.LearningHistoryId == learningHistory.Id,
                cancellationToken);
            
        if (completedLesson != null)
        {
            _context.CompletedLessons.Remove(completedLesson);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<LearningHistory> GetLearningHistoryByStudentIdAsync(Guid studentId)
    {
        return await _context.LearningHistories
            .Include(lh => lh.CourseProgresses)
            .FirstOrDefaultAsync(lh => lh.StudentId == studentId);
    }

    public async Task<CourseProgress> GetCourseProgressByIdAsync(Guid courseProgressId)
    {
        return await _context.CourseProgresses
            .Include(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(cp => cp.Id == courseProgressId);
    }

    public async Task<IEnumerable<CourseProgress>> GetCourseProgressesByStudentIdAsync(Guid studentId)
    {
        var learningHistory = await _context.LearningHistories
            .Include(lh => lh.CourseProgresses)
            .ThenInclude(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(lh => lh.StudentId == studentId);

        return learningHistory?.CourseProgresses ?? new List<CourseProgress>();
    }

    public async Task<IEnumerable<CompletedLesson>> GetCompletedLessonsByCourseProgressIdAsync(Guid courseProgressId)
    {
        var courseProgress = await _context.CourseProgresses
            .Include(cp => cp.CompletedLessons)
            .FirstOrDefaultAsync(cp => cp.Id == courseProgressId);

        return courseProgress?.CompletedLessons ?? new List<CompletedLesson>();
    }
} 