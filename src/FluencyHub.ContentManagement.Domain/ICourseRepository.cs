namespace FluencyHub.ContentManagement.Domain;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(Guid id);
    Task<IEnumerable<Course>> GetAllAsync();
    Task<IEnumerable<Course>> GetActiveCoursesAsync();
    Task AddAsync(Course course);
    Task UpdateAsync(Course course);
    Task<bool> DeleteAsync(Guid id);
} 