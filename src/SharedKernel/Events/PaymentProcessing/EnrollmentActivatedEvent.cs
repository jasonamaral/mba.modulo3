namespace FluencyHub.SharedKernel.Events.PaymentProcessing;

/// <summary>
/// Evento disparado quando uma matrícula é ativada após pagamento
/// </summary>
public class EnrollmentActivatedEvent : DomainEventBase
{
    public Guid EnrollmentId { get; }
    public Guid StudentId { get; }
    public Guid CourseId { get; }
    public DateTime ActivationDate { get; }

    public EnrollmentActivatedEvent(
        Guid enrollmentId,
        Guid studentId,
        Guid courseId,
        DateTime activationDate)
    {
        EnrollmentId = enrollmentId;
        StudentId = studentId;
        CourseId = courseId;
        ActivationDate = activationDate;
    }
} 