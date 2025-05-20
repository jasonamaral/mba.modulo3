using FluencyHub.Application.Common.Exceptions;
using FluencyHub.PaymentProcessing.Application.Commands.ProcessPayment;
using FluencyHub.PaymentProcessing.Application.Commands.RefundPayment;
using FluencyHub.PaymentProcessing.Application.Queries.GetPaymentById;

namespace FluencyHub.Application.PaymentProcessing;

// Esta classe serve apenas para reexportar tipos do namespace FluencyHub.PaymentProcessing.Application
// para que eles possam ser usados através do namespace FluencyHub.Application.PaymentProcessing
public static class PaymentProcessingExports
{
    // Commands
    public record ProcessPaymentCommand : FluencyHub.PaymentProcessing.Application.Commands.ProcessPayment.ProcessPaymentCommand { }
    public record RefundPaymentCommand : FluencyHub.PaymentProcessing.Application.Commands.RefundPayment.RefundPaymentCommand { }

    // Queries and DTOs
    public class PaymentDto : FluencyHub.PaymentProcessing.Application.Queries.GetPaymentById.PaymentDto { }

    // Command handlers and other types are automatically resolved through dependency injection
} 