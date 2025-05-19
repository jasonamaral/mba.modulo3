using FluencyHub.StudentManagement.Domain;

namespace FluencyHub.Application.Common.Interfaces;

public interface ICertificateRepository
{
    Task<Certificate?> GetByIdAsync(Guid id);
    Task<IEnumerable<Certificate>> GetByStudentIdAsync(Guid studentId);
    Task<IEnumerable<Certificate>> GetByCourseIdAsync(Guid courseId);
    Task<Certificate?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId);
    Task<IEnumerable<Certificate>> GetAllAsync();
    Task AddAsync(Certificate certificate);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}