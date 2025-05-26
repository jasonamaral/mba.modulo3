using FluencyHub.PaymentProcessing.Domain;
using FluencyHub.PaymentProcessing.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FluencyHub.PaymentProcessing.Infrastructure.Persistence.Repositories;

public class PaymentRepository : FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentRepository
{
    private readonly PaymentDbContext _dbContext;
    private readonly FluencyHub.SharedKernel.Events.IDomainEventService _eventService;

    public PaymentRepository(PaymentDbContext dbContext, FluencyHub.SharedKernel.Events.IDomainEventService eventService)
    {
        _dbContext = dbContext;
        _eventService = eventService;
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Payment>> GetByEnrollmentIdAsync(Guid enrollmentId)
    {
        return await _dbContext.Payments
            .Where(p => p.EnrollmentId == enrollmentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByStudentIdAsync(Guid studentId)
    {
        return await _dbContext.Payments
            .Where(p => p.StudentId == studentId)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Payment>> GetByTransactionIdAsync(string transactionId)
    {
        return await _dbContext.Payments
            .Where(p => p.TransactionId == transactionId)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Payment>> GetSuccessfulPaymentsAsync()
    {
        return await _dbContext.Payments
            .Where(p => p.Status == StatusPagamento.Aprovado)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _dbContext.Payments.ToListAsync();
    }

    public async Task AddAsync(Payment payment)
    {
        await _dbContext.Payments.AddAsync(payment);
    }

    public async Task UpdateAsync(Payment payment)
    {
        _dbContext.Payments.Update(payment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var payment = await _dbContext.Payments.FindAsync(id);
        if (payment == null)
        {
            throw new NotFoundException(nameof(Payment), id);
        }

        _dbContext.Payments.Remove(payment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
} 