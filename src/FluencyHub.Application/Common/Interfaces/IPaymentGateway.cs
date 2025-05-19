using FluencyHub.PaymentProcessing.Domain;

namespace FluencyHub.Application.Common.Interfaces;

public interface IPaymentGateway
{
    Task<FluencyHub.Application.Common.Models.PaymentResult> ProcessPaymentAsync(string orderId, decimal amount, CardDetails cardDetails);

    Task<FluencyHub.Application.Common.Models.RefundResult> ProcessRefundAsync(string transactionId, decimal amount, string reason);
}