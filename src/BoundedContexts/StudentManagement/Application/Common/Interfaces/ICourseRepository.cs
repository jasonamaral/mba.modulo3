namespace FluencyHub.StudentManagement.Application.Common.Interfaces;

public interface ICourseRepository
{
    Task<CourseInfo?> GetByIdAsync(Guid id);
    Task<bool> ExistsAsync(Guid courseId);
    Task<string> GetNameAsync(Guid courseId);
}

public class CourseInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
} 