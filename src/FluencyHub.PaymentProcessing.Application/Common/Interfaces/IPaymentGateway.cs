using FluencyHub.Domain.PaymentProcessing;
using FluencyHub.PaymentProcessing.Application.Common.Models;

namespace FluencyHub.PaymentProcessing.Application.Common.Interfaces;

public interface IPaymentGateway
{
    Task<FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult> ProcessPaymentAsync(string orderId, decimal amount, CardDetails cardDetails);
    Task<FluencyHub.PaymentProcessing.Application.Common.Models.RefundResult> ProcessRefundAsync(string transactionId, decimal amount, string reason);
} 