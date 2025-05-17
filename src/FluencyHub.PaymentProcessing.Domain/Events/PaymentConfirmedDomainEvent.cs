using FluencyHub.Common.Domain;

namespace FluencyHub.PaymentProcessing.Domain.Events;

public class PaymentConfirmedDomainEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public Guid EnrollmentId { get; }
    public string TransactionId { get; }

    public PaymentConfirmedDomainEvent(Guid paymentId, Guid enrollmentId, string transactionId)
    {
        PaymentId = paymentId;
        EnrollmentId = enrollmentId;
        TransactionId = transactionId;
    }
} 