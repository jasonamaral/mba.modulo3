using FluencyHub.PaymentProcessing.Domain.Common;
using FluencyHub.PaymentProcessing.Domain.Events;
using FluencyHub.SharedKernel.Contracts;
using System.Text.Json.Serialization;

namespace FluencyHub.PaymentProcessing.Domain;

public class Payment : BaseEntity
{
    public Guid StudentId { get; private init; }
    public Guid EnrollmentId { get; private init; }
    public decimal Amount { get; private init; }
    public StatusPagamento Status { get; private set; }
    public string? TransactionId { get; private set; }
    public string? FailureReason { get; private set; }
    public string? RefundReason { get; private set; }
    public required CardDetails CardDetails { get; init; }
    public DateTime PaymentDate { get; private init; }

    [JsonIgnore]
    public IEnrollment? Enrollment { get; private set; }

    private Payment()
    { }

    public Payment(Guid studentId, Guid enrollmentId, decimal amount, CardDetails cardDetails)
    {
        if (amount <= 0)
            throw new ArgumentException("O valor do pagamento deve ser positivo", nameof(amount));

        StudentId = studentId;
        EnrollmentId = enrollmentId;
        Amount = amount;
        CardDetails = cardDetails ?? throw new ArgumentException("Os detalhes do cartão não podem ser nulos", nameof(cardDetails));
        Status = StatusPagamento.Pendente;
        PaymentDate = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsSuccess(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("O ID da transação não pode estar vazio", nameof(transactionId));

        if (Status != StatusPagamento.Pendente)
            throw new InvalidOperationException($"Não é possível marcar como bem-sucedido um pagamento com status {Status}");

        TransactionId = transactionId;
        Status = StatusPagamento.Aprovado;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentConfirmedDomainEvent(Id, EnrollmentId, transactionId));
    }

    public void MarkAsFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A razão da falha não pode estar vazia", nameof(reason));

        if (Status != StatusPagamento.Pendente)
            throw new InvalidOperationException($"Não é possível marcar como falho um pagamento com status {Status}");

        Status = StatusPagamento.Falha;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsRefunded(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A razão do reembolso não pode estar vazia", nameof(reason));

        if (Status != StatusPagamento.Aprovado)
            throw new InvalidOperationException($"Não é possível reembolsar um pagamento com status {Status}");

        Status = StatusPagamento.Reembolsado;
        RefundReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsSuccessful => Status == StatusPagamento.Aprovado;
    public bool IsFailed => Status == StatusPagamento.Falha;
    public bool IsPending => Status == StatusPagamento.Pendente;
    public bool IsRefunded => Status == StatusPagamento.Reembolsado;
}

public enum StatusPagamento
{
    Pendente,
    Aprovado,
    Falha,
    Reembolsado
} 