using MediatR;

namespace FluencyHub.SharedKernel.Events.Integration;

public class PaymentProcessedEvent : INotification
{
    public Guid PaymentId { get; }
    public Guid EnrollmentId { get; }
    public Guid StudentId { get; }
    public decimal Amount { get; }
    public string Status { get; }
    public DateTime ProcessedAt { get; }

    public PaymentProcessedEvent(Guid paymentId, Guid enrollmentId, Guid studentId, decimal amount, string status, DateTime processedAt)
    {
        PaymentId = paymentId;
        EnrollmentId = enrollmentId;
        StudentId = studentId;
        Amount = amount;
        Status = status;
        ProcessedAt = processedAt;
    }
} 