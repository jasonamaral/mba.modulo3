using FluencyHub.StudentManagement.Domain;

namespace FluencyHub.StudentManagement.Application.Common.Interfaces;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId);
    Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId);
    Task<Enrollment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId);
    Task AddAsync(Enrollment enrollment);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
} 