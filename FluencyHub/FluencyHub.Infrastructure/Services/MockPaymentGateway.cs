using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.PaymentProcessing;
using Microsoft.Extensions.Logging;

namespace FluencyHub.Infrastructure.Services;

public class MockPaymentGateway : IPaymentGateway
{
    private readonly ILogger<MockPaymentGateway> _logger;
    private static readonly Dictionary<string, string> _transactions = new();

    public MockPaymentGateway(ILogger<MockPaymentGateway> logger)
    {
        _logger = logger;
    }

    public Task<Application.Common.Interfaces.PaymentResult> ProcessPaymentAsync(string orderId, decimal amount, CardDetails cardDetails)
    {
        _logger.LogInformation("MockPaymentGateway: Processing payment for order {OrderId}, amount {Amount}", orderId, amount);

        // Simular validação do cartão
        if (cardDetails.MaskedCardNumber.Contains("4111") || cardDetails.MaskedCardNumber.Contains("1111"))
        {
            var transactionId = $"mock_txn_{Guid.NewGuid():N}";
            _transactions[transactionId] = orderId;

            _logger.LogInformation("MockPaymentGateway: Payment successful, transaction ID: {TransactionId}", transactionId);
            return Task.FromResult(Application.Common.Interfaces.PaymentResult.Success(transactionId));
        }
        else
        {
            _logger.LogWarning("MockPaymentGateway: Payment failed, invalid card number");
            return Task.FromResult(Application.Common.Interfaces.PaymentResult.Failure("Card was declined"));
        }
    }

    public Task<Application.Common.Interfaces.RefundResult> ProcessRefundAsync(string transactionId, decimal amount, string reason)
    {
        _logger.LogInformation("MockPaymentGateway: Processing refund for transaction {TransactionId}, amount {Amount}, reason: {Reason}", 
            transactionId, amount, reason);

        if (_transactions.ContainsKey(transactionId))
        {
            var refundTransactionId = $"mock_refund_{Guid.NewGuid():N}";
            
            _logger.LogInformation("MockPaymentGateway: Refund successful, refund ID: {RefundId}", refundTransactionId);
            return Task.FromResult(Application.Common.Interfaces.RefundResult.Success(
                transactionId, 
                refundTransactionId, 
                amount));
        }
        else
        {
            _logger.LogWarning("MockPaymentGateway: Refund failed, transaction not found");
            return Task.FromResult(Application.Common.Interfaces.RefundResult.Failure(
                transactionId, 
                "Transaction not found"));
        }
    }
} 