namespace FluencyHub.SharedKernel.Events.PaymentProcessing;

/// <summary>
/// Evento disparado quando um pagamento é processado
/// </summary>
public class PaymentProcessedEvent : DomainEventBase
{
    public Guid PaymentId { get; }
    public Guid EnrollmentId { get; }
    public Guid StudentId { get; }
    public Guid CourseId { get; }
    public decimal Amount { get; }
    public DateTime PaymentDate { get; }
    public string TransactionId { get; }
    public bool IsSuccessful { get; }
    public string? ErrorMessage { get; }

    public PaymentProcessedEvent(
        Guid paymentId,
        Guid enrollmentId,
        Guid studentId,
        Guid courseId,
        decimal amount,
        DateTime paymentDate,
        string transactionId,
        bool isSuccessful,
        string? errorMessage = null)
    {
        PaymentId = paymentId;
        EnrollmentId = enrollmentId;
        StudentId = studentId;
        CourseId = courseId;
        Amount = amount;
        PaymentDate = paymentDate;
        TransactionId = transactionId;
        IsSuccessful = isSuccessful;
        ErrorMessage = errorMessage;
    }
} 