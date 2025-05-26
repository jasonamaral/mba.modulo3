using FluencyHub.PaymentProcessing.Domain;
using FluencyHub.PaymentProcessing.Application.Common.Models;

namespace FluencyHub.PaymentProcessing.Application.Common.Interfaces;

public interface IPaymentGateway
{
    Task<FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult> ProcessPaymentAsync(string orderId, decimal amount, Models.CardDetails cardDetails);
    Task<RefundResult> ProcessRefundAsync(string transactionId, decimal amount, string reason);
} 