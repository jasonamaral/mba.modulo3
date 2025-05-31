using FluencyHub.StudentManagement.Domain;

namespace FluencyHub.StudentManagement.Application.Common.Interfaces;

public interface ILearningRepository
{
    Task<LearningHistory> GetLearningHistoryByStudentIdAsync(Guid studentId);
    Task<LearningHistory?> GetByStudentIdAsync(Guid studentId);
    Task<LearningHistory?> GetByStudentIdFreshAsync(Guid studentId);
    Task<CourseProgress> GetCourseProgressAsync(Guid courseId, Guid learningHistoryId);
    Task<CourseProgress> GetCourseProgressByIdAsync(Guid courseProgressId);
    Task<IEnumerable<CourseProgress>> GetCourseProgressesByStudentIdAsync(Guid studentId);
    Task<IEnumerable<CompletedLesson>> GetCompletedLessonsByCourseProgressIdAsync(Guid courseProgressId);
    Task<IEnumerable<Guid>> GetCompletedLessonIdsAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default);
    Task AddLearningHistoryAsync(LearningHistory learningHistory);
    Task AddCourseProgressAsync(CourseProgress courseProgress);
    Task AddCompletedLessonAsync(CompletedLesson completedLesson);
    Task CompleteLessonAsync(Guid studentId, Guid courseId, Guid lessonId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
} 