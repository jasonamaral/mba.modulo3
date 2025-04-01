using MediatR;

namespace FluencyHub.Application.PaymentProcessing.Queries.GetPaymentById;

public record GetPaymentByIdQuery(Guid Id) : IRequest<PaymentDto>;

public record PaymentDto
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public Guid EnrollmentId { get; init; }
    public decimal Amount { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? TransactionId { get; init; }
    public string? FailureReason { get; init; }
    public string? RefundReason { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
} 