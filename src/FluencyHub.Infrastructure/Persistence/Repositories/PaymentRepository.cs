using FluencyHub.Domain.PaymentProcessing;
using Microsoft.EntityFrameworkCore;

namespace FluencyHub.Infrastructure.Persistence.Repositories;

public class PaymentRepository : Application.Common.Interfaces.IPaymentRepository
{
    private readonly FluencyHubDbContext _context;

    public PaymentRepository(FluencyHubDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Payment>> GetByStudentIdAsync(Guid studentId)
    {
        return await _context.Payments
            .Include(p => p.Enrollment)
            .Where(p => p.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByEnrollmentIdAsync(Guid enrollmentId)
    {
        return await _context.Payments
            .Where(p => p.EnrollmentId == enrollmentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByTransactionIdAsync(string transactionId)
    {
        return await _context.Payments
            .Where(p => p.TransactionId == transactionId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetSuccessfulPaymentsAsync()
    {
        return await _context.Payments
            .Include(p => p.Enrollment)
            .Where(p => p.Status == PaymentStatus.Successful)
            .ToListAsync();
    }

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}