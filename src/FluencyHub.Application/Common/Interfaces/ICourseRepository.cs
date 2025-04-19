using FluencyHub.Domain.ContentManagement;

namespace FluencyHub.Application.Common.Interfaces;

public interface ICourseRepository
{
    Task<Course> GetByIdAsync(Guid id);
    Task<Course> GetByIdWithLessonsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetAllAsync();
    Task<IEnumerable<Course>> GetActiveCoursesAsync();
    Task<IEnumerable<Course>> GetByLanguageAsync(string language);
    Task<IEnumerable<Course>> GetByLevelAsync(string level);
    Task<Course> AddAsync(Course course);
    Task UpdateAsync(Course course);
    Task DeleteAsync(Guid id);
    Task<Lesson> GetLessonByIdAsync(Guid lessonId);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CourseProgressInfo>> GetCourseProgressesForStudent(Guid studentId, CancellationToken cancellationToken = default);
    Task<int> GetLessonsCountByCourseId(Guid courseId, CancellationToken cancellationToken = default);
} 