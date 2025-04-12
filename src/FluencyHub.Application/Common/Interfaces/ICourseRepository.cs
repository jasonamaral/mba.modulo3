using FluencyHub.Domain.ContentManagement;

namespace FluencyHub.Application.Common.Interfaces;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(Guid id);
    Task<Course?> GetByIdWithLessonsAsync(Guid id);
    Task<IEnumerable<Course>> GetAllAsync();
    Task<IEnumerable<Course>> GetActiveCoursesAsync();
    Task<IEnumerable<Course>> GetByLanguageAsync(string language);
    Task<IEnumerable<Course>> GetByLevelAsync(string level);
    Task AddAsync(Course course);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
} 