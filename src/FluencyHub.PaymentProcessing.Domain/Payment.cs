using FluencyHub.Common.Domain;
using FluencyHub.PaymentProcessing.Domain.Events;
using System.Text.Json.Serialization;

namespace FluencyHub.PaymentProcessing.Domain;

public class Payment : BaseEntity
{
    public Guid StudentId { get; private set; }
    public Guid EnrollmentId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? TransactionId { get; private set; }
    public string? FailureReason { get; private set; }
    public string? RefundReason { get; private set; }
    public CardDetails CardDetails { get; private set; }
    public DateTime PaymentDate { get; private set; }

    private Payment()
    { }

    public Payment(Guid studentId, Guid enrollmentId, decimal amount, CardDetails cardDetails)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be positive", nameof(amount));

        StudentId = studentId;
        EnrollmentId = enrollmentId;
        Amount = amount;
        CardDetails = cardDetails ?? throw new ArgumentException("Card details cannot be null", nameof(cardDetails));
        Status = PaymentStatus.Pending;
        PaymentDate = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsSuccess(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID cannot be empty", nameof(transactionId));

        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot mark as success a payment with status {Status}");

        Status = PaymentStatus.Successful;
        TransactionId = transactionId;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new PaymentConfirmedDomainEvent(Id, EnrollmentId, transactionId));
    }

    public void MarkAsFailed(string reason)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot mark as failed a payment with status {Status}");

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new PaymentRejectedDomainEvent(Id, EnrollmentId, reason));
    }

    public void MarkAsRefunded(string reason)
    {
        if (Status != PaymentStatus.Successful)
            throw new InvalidOperationException("Only successful payments can be refunded");

        Status = PaymentStatus.Refunded;
        RefundReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsSuccessful => Status == PaymentStatus.Successful;
    public bool IsFailed => Status == PaymentStatus.Failed;
    public bool IsPending => Status == PaymentStatus.Pending;
    public bool IsRefunded => Status == PaymentStatus.Refunded;
}

public enum PaymentStatus
{
    Pending,
    Successful,
    Failed,
    Refunded
} 