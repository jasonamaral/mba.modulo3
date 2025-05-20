using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.Common.Models;
using FluencyHub.PaymentProcessing.Domain;
using FluencyHub.PaymentProcessing.Infrastructure.Services;

namespace FluencyHub.API.Adapters;

public class PaymentGatewayAdapter : IPaymentGateway
{
    private readonly FluencyHub.PaymentProcessing.Infrastructure.Services.MockPaymentGateway _gateway;

    public PaymentGatewayAdapter(MockPaymentGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<FluencyHub.Application.Common.Models.PaymentResult> ProcessPaymentAsync(string orderId, decimal amount, CardDetails cardDetails)
    {
        var result = await _gateway.ProcessPaymentAsync(orderId, amount, cardDetails);
        
        // Converter o resultado do gateway para o formato esperado pela interface da aplicação
        if (result.IsSuccessful)
        {
            return FluencyHub.Application.Common.Models.PaymentResult.Success(result.TransactionId!);
        }
        else
        {
            return FluencyHub.Application.Common.Models.PaymentResult.Failure(result.ErrorMessage ?? "Unknown error");
        }
    }

    public async Task<FluencyHub.Application.Common.Models.RefundResult> ProcessRefundAsync(string transactionId, decimal amount, string reason)
    {
        var result = await _gateway.ProcessRefundAsync(transactionId, amount, reason);
        
        // Converter o resultado do gateway para o formato esperado pela interface da aplicação
        if (result.IsSuccessful)
        {
            return FluencyHub.Application.Common.Models.RefundResult.Success(
                result.OriginalTransactionId, 
                result.RefundTransactionId!, 
                result.RefundAmount);
        }
        else
        {
            return FluencyHub.Application.Common.Models.RefundResult.Failure(
                result.OriginalTransactionId, 
                result.ErrorMessage ?? "Unknown error");
        }
    }
} 