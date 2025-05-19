using FluencyHub.StudentManagement.Domain;

namespace FluencyHub.StudentManagement.Application.Common.Interfaces;

public interface ILearningRepository
{
    Task<LearningHistory?> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<CourseProgress?> GetCourseProgressAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default);
    Task<bool> HasCompletedLessonAsync(Guid studentId, Guid lessonId, CancellationToken cancellationToken = default);
    Task<int> GetCompletedLessonsCountAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> GetCompletedLessonIdsAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default);
    Task AddAsync(LearningHistory learningHistory, CancellationToken cancellationToken = default);
    Task UpdateAsync(LearningHistory learningHistory, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task CompleteLessonAsync(Guid studentId, Guid courseId, Guid lessonId, CancellationToken cancellationToken = default);
    Task UncompleteLessonAsync(Guid studentId, Guid lessonId, CancellationToken cancellationToken = default);
} 