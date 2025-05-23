using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluencyHub.SharedKernel.Contracts;

public interface IEnrollmentRepository
{
    Task<IEnrollment?> GetByIdAsync(Guid id);
    Task<IEnrollment?> GetByPaymentIdAsync(Guid paymentId);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
} 