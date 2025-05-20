using FluencyHub.Application.Common.Interfaces;
using FluencyHub.PaymentProcessing.Domain;
using FluencyHub.PaymentProcessing.Infrastructure.Persistence.Repositories;

namespace FluencyHub.API.Adapters;

public class PaymentRepositoryAdapter : FluencyHub.Application.Common.Interfaces.IPaymentRepository
{
    private readonly PaymentRepository _repository;

    public PaymentRepositoryAdapter(PaymentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Payment>> GetByStudentIdAsync(Guid studentId)
    {
        return await _repository.GetByStudentIdAsync(studentId);
    }

    public async Task<IEnumerable<Payment>> GetByEnrollmentIdAsync(Guid enrollmentId)
    {
        return await _repository.GetByEnrollmentIdAsync(enrollmentId);
    }

    public async Task<IEnumerable<Payment>> GetByTransactionIdAsync(string transactionId)
    {
        return await _repository.GetByTransactionIdAsync(transactionId);
    }

    public async Task<IEnumerable<Payment>> GetSuccessfulPaymentsAsync()
    {
        return await _repository.GetSuccessfulPaymentsAsync();
    }

    public async Task AddAsync(Payment payment)
    {
        await _repository.AddAsync(payment);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _repository.SaveChangesAsync(cancellationToken);
    }
} 