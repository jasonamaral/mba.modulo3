using MediatR;

namespace FluencyHub.PaymentProcessing.Application.Commands.RefundPayment;

public record RefundPaymentCommand : IRequest<bool>
{
    public required Guid PaymentId { get; init; }
    public required decimal Amount { get; init; }
    public string? Reason { get; init; }
} 