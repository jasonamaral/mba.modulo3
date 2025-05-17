using FluencyHub.Common.Domain;

namespace FluencyHub.PaymentProcessing.Domain.Events;

public class PaymentRejectedDomainEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public Guid EnrollmentId { get; }
    public string FailureReason { get; }

    public PaymentRejectedDomainEvent(Guid paymentId, Guid enrollmentId, string failureReason)
    {
        PaymentId = paymentId;
        EnrollmentId = enrollmentId;
        FailureReason = failureReason;
    }
} 