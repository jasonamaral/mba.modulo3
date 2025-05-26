using FluencyHub.StudentManagement.Domain;

namespace FluencyHub.StudentManagement.Application.Common.Interfaces;

public interface ILearningRepository
{
    Task<LearningHistory> GetLearningHistoryByStudentIdAsync(Guid studentId);
    Task<CourseProgress> GetCourseProgressAsync(Guid courseId, Guid learningHistoryId);
    Task<CourseProgress> GetCourseProgressByIdAsync(Guid courseProgressId);
    Task<IEnumerable<CourseProgress>> GetCourseProgressesByStudentIdAsync(Guid studentId);
    Task<IEnumerable<CompletedLesson>> GetCompletedLessonsByCourseProgressIdAsync(Guid courseProgressId);
    Task AddLearningHistoryAsync(LearningHistory learningHistory);
    Task AddCourseProgressAsync(CourseProgress courseProgress);
    Task AddCompletedLessonAsync(CompletedLesson completedLesson);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
} 