using MediatR;

namespace FluencyHub.Application.PaymentProcessing.Commands.RefundPayment;

public record RefundPaymentCommand : IRequest
{
    public Guid PaymentId { get; init; }
    public string Reason { get; init; } = string.Empty;
} 