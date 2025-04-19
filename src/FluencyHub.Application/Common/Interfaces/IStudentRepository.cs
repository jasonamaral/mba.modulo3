using FluencyHub.Domain.StudentManagement;

namespace FluencyHub.Application.Common.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id);
    Task<Student?> GetByEmailAsync(string email);
    Task<IEnumerable<Student>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<Student>> GetActiveStudentsAsync();
    Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(Guid studentId);
    Task<IEnumerable<Certificate>> GetCertificatesByStudentIdAsync(Guid studentId);
    Task AddAsync(Student student);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
} 