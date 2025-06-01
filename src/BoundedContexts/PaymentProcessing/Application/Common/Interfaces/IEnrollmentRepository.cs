using FluencyHub.SharedKernel.Contracts;

namespace FluencyHub.PaymentProcessing.Application.Common.Interfaces;

public interface IEnrollmentRepository
{
    Task<IEnrollment?> GetByIdAsync(Guid id);
    Task<IEnrollment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId);
    Task<IEnrollment?> GetPendingEnrollmentAsync(Guid studentId, Guid courseId);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
} 