using FluencyHub.StudentManagement.Domain;

namespace FluencyHub.StudentManagement.Application.Common.Interfaces;

public interface IStudentManagementEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId);
    Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId);
    Task<IEnumerable<Enrollment>> GetAllAsync();
    Task<IEnumerable<Enrollment>> GetActiveEnrollmentsAsync();
    Task<Enrollment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId);
    Task AddAsync(Enrollment enrollment);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> HasActiveEnrollmentAsync(Guid studentId, Guid courseId);
    Task<IEnumerable<Enrollment>> GetEnrollmentsWithStudentAndCourseAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Enrollment enrollment);
} 