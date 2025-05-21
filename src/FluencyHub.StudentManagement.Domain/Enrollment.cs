using FluencyHub.StudentManagement.Domain.Common;
using FluencyHub.ContentManagement.Domain;
using System.Text.Json.Serialization;

namespace FluencyHub.StudentManagement.Domain;

public class Enrollment : BaseEntity
{
    public Guid StudentId { get; private set; }
    public Guid CourseId { get; private set; }
    public decimal Price { get; private set; }
    public StatusMatricula Status { get; private set; }
    public DateTime EnrollmentDate { get; private set; }
    public DateTime? ActivationDate { get; private set; }
    public DateTime? CompletionDate { get; private set; }
    public Guid? PaymentId { get; private set; }
    public string? PaymentFailureReason { get; private set; }

    [JsonIgnore]
    public Student Student { get; private set; }

    [JsonIgnore]
    public Course Course { get; private set; }

    private Enrollment()
    { }

    public Enrollment(Guid studentId, Guid courseId, decimal price)
    {
        if (price < 0)
            throw new ArgumentException("O preço não pode ser negativo", nameof(price));

        StudentId = studentId;
        CourseId = courseId;
        Price = price;
        Status = StatusMatricula.AguardandoPagamento;
        EnrollmentDate = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public void ActivateEnrollment()
    {
        if (Status != StatusMatricula.AguardandoPagamento)
            throw new InvalidOperationException($"Não é possível ativar uma matrícula com status {Status}");

        Status = StatusMatricula.Ativa;
        ActivationDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CompleteEnrollment()
    {
        if (Status != StatusMatricula.Ativa)
            throw new InvalidOperationException($"Não é possível completar uma matrícula com status {Status}. A matrícula deve estar ativa.");

        Status = StatusMatricula.Concluida;
        CompletionDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CancelEnrollment()
    {
        if (Status == StatusMatricula.Concluida)
            throw new InvalidOperationException("Não é possível cancelar uma matrícula concluída");

        Status = StatusMatricula.Cancelada;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void ProcessPaymentSuccess(Guid paymentId, decimal amount)
    {
        if (Status != StatusMatricula.AguardandoPagamento)
            throw new InvalidOperationException($"Não é possível processar o pagamento para uma matrícula com status {Status}");
            
        PaymentId = paymentId;
        Status = StatusMatricula.Ativa;
        ActivationDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void ProcessPaymentFailure(string errorMessage)
    {
        if (Status != StatusMatricula.AguardandoPagamento)
            throw new InvalidOperationException($"Não é possível processar a falha de pagamento para uma matrícula com status {Status}");
            
        PaymentFailureReason = errorMessage;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsActive => Status == StatusMatricula.Ativa;
    public bool IsPendingPayment => Status == StatusMatricula.AguardandoPagamento;
    public bool IsCompleted => Status == StatusMatricula.Concluida;
    public bool IsCancelled => Status == StatusMatricula.Cancelada;
}

public enum StatusMatricula
{
    AguardandoPagamento,
    Ativa,
    Concluida,
    Cancelada
} 