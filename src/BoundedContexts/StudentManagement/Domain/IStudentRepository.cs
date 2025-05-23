namespace FluencyHub.StudentManagement.Domain;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id);
    Task<Student?> GetByEmailAsync(string email);
    Task<IEnumerable<Student>> GetAllAsync();
    Task<IEnumerable<Student>> GetActiveStudentsAsync();
    Task AddAsync(Student student);
    Task UpdateAsync(Student student);
    Task<bool> DeleteAsync(Guid id);
} 