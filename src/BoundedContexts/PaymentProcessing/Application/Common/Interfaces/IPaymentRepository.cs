using FluencyHub.PaymentProcessing.Domain;

namespace FluencyHub.PaymentProcessing.Application.Common.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Payment>> GetByStudentIdAsync(Guid studentId);
    Task<IEnumerable<Payment>> GetByEnrollmentIdAsync(Guid enrollmentId);
    Task<IEnumerable<Payment>> GetByTransactionIdAsync(string transactionId);
    Task<IEnumerable<Payment>> GetSuccessfulPaymentsAsync();
    Task AddAsync(Payment payment);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}