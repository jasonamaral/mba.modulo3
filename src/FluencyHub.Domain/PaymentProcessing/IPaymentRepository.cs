namespace FluencyHub.Domain.PaymentProcessing;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Payment>> GetByStudentIdAsync(Guid studentId);
    Task<Payment?> GetByEnrollmentIdAsync(Guid enrollmentId);
    Task AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
} 