using FluencyHub.PaymentProcessing.Application.Common.Interfaces;
using FluencyHub.PaymentProcessing.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace FluencyHub.PaymentProcessing.Infrastructure.Services;

public class MockPaymentGateway : IPaymentGateway
{
    private readonly ILogger<MockPaymentGateway> _logger;
    private readonly IPaymentRepository _paymentRepository;
    private static readonly Dictionary<string, string> _transactions = new();

    public MockPaymentGateway(ILogger<MockPaymentGateway> logger, IPaymentRepository paymentRepository)
    {
        _logger = logger;
        _paymentRepository = paymentRepository;
    }

    public Task<PaymentResult> ProcessPaymentAsync(string orderId, decimal amount, CardDetails cardDetails)
    {

        if (cardDetails.MaskedCardNumber.Contains("4111") || cardDetails.MaskedCardNumber.Contains("1111"))
        {
            var transactionId = $"mock_txn_{Guid.NewGuid():N}";
            _transactions[transactionId] = orderId;

            return Task.FromResult(PaymentResult.Success(transactionId));
        }
        else
        {
            return Task.FromResult(PaymentResult.Failure("Cartão foi recusado"));
        }
    }

    public async Task<RefundResult> ProcessRefundAsync(string transactionId, decimal amount, string reason)
    {

        if (_transactions.ContainsKey(transactionId))
        {
            var refundTransactionId = $"mock_refund_{Guid.NewGuid():N}";
            return RefundResult.Success(transactionId, refundTransactionId, amount);
        }

        try
        {
            var paymentsWithTransaction = await _paymentRepository.GetByTransactionIdAsync(transactionId);
            var payment = paymentsWithTransaction.FirstOrDefault();

            if (payment != null && payment.Status == Domain.StatusPagamento.Aprovado)
            {
                var refundTransactionId = $"mock_refund_{Guid.NewGuid():N}";

                return RefundResult.Success(transactionId, refundTransactionId, amount);
            }
            else if (payment != null)
            {
                return RefundResult.Failure(transactionId, $"Pagamento não está em status aprovado: {payment.Status}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MockPaymentGateway: Error checking database for transaction {TransactionId}", transactionId);
        }

        return RefundResult.Failure(transactionId, "Transação não encontrada");
    }
}