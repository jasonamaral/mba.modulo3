using MediatR;

namespace FluencyHub.PaymentProcessing.Application.Commands.ProcessPayment;

public record ProcessPaymentCommand : IRequest<Guid>
{
    public required Guid StudentId { get; init; }
    public required Guid EnrollmentId { get; init; }
    public required decimal Amount { get; init; }
    public required string PaymentMethod { get; init; }
    public required string CardNumber { get; init; }
    public required string CardHolderName { get; init; }
    public required string ExpirationDate { get; init; }
    public required string SecurityCode { get; init; }
} 