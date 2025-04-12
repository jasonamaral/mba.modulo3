using MediatR;

namespace FluencyHub.Application.PaymentProcessing.Commands.ProcessPayment;

public record ProcessPaymentCommand : IRequest<Guid>
{
    public Guid EnrollmentId { get; init; }
    public string CardHolderName { get; init; } = string.Empty;
    public string CardNumber { get; init; } = string.Empty;
    public string ExpiryMonth { get; init; } = string.Empty;
    public string ExpiryYear { get; init; } = string.Empty;
} 