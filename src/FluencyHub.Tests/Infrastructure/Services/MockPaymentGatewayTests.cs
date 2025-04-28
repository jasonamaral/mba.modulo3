using FluencyHub.Application.Common.Models;
using FluencyHub.Domain.PaymentProcessing;
using FluencyHub.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace FluencyHub.Tests.Infrastructure.Services;

public class MockPaymentGatewayTests
{
    private readonly Mock<ILogger<MockPaymentGateway>> _loggerMock;
    private readonly MockPaymentGateway _gateway;
    private readonly CardDetails _validCardDetails;
    private readonly CardDetails _invalidCardDetails;

    public MockPaymentGatewayTests()
    {
        _loggerMock = new Mock<ILogger<MockPaymentGateway>>();
        _gateway = new MockPaymentGateway(_loggerMock.Object);
        
        _validCardDetails = new CardDetails(
            "Jane Doe",
            "4111111111111111", // Visa válido
            "12",
            "2030"
        );
        
        _invalidCardDetails = new CardDetails(
            "John Smith",
            "4242424242424242", // Número válido pelo Luhn, mas será recusado pelo gateway
            "12",
            "2030"
        );
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithValidCard_ReturnsSuccessResult()
    {
        // Arrange
        var orderId = Guid.NewGuid().ToString();
        var amount = 100.00m;

        // Act
        var result = await _gateway.ProcessPaymentAsync(orderId, amount, _validCardDetails);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.TransactionId);
        Assert.StartsWith("mock_txn_", result.TransactionId);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithInvalidCard_ReturnsFailureResult()
    {
        // Arrange
        var orderId = Guid.NewGuid().ToString();
        var amount = 100.00m;

        // Act
        var result = await _gateway.ProcessPaymentAsync(orderId, amount, _invalidCardDetails);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Null(result.TransactionId);
        Assert.Equal("Card was declined", result.ErrorMessage);
    }

    [Fact]
    public async Task ProcessRefundAsync_WithValidTransaction_ReturnsSuccessResult()
    {
        // Arrange
        var orderId = Guid.NewGuid().ToString();
        var amount = 100.00m;
        
        // Primeiro processa um pagamento válido para obter um ID de transação
        var paymentResult = await _gateway.ProcessPaymentAsync(orderId, amount, _validCardDetails);
        var transactionId = paymentResult.TransactionId;
        var refundAmount = 50.00m;
        var reason = "Customer requested";

        // Act
        var refundResult = await _gateway.ProcessRefundAsync(transactionId!, refundAmount, reason);

        // Assert
        Assert.True(refundResult.IsSuccessful);
        Assert.Equal(transactionId, refundResult.OriginalTransactionId);
        Assert.NotNull(refundResult.RefundTransactionId);
        Assert.StartsWith("mock_refund_", refundResult.RefundTransactionId);
        Assert.Equal(refundAmount, refundResult.RefundAmount);
        Assert.Null(refundResult.ErrorMessage);
    }

    [Fact]
    public async Task ProcessRefundAsync_WithInvalidTransaction_ReturnsFailureResult()
    {
        // Arrange
        var invalidTransactionId = "invalid_transaction_id";
        var refundAmount = 50.00m;
        var reason = "Customer requested";

        // Act
        var refundResult = await _gateway.ProcessRefundAsync(invalidTransactionId, refundAmount, reason);

        // Assert
        Assert.False(refundResult.IsSuccessful);
        Assert.Equal(invalidTransactionId, refundResult.OriginalTransactionId);
        Assert.Null(refundResult.RefundTransactionId);
        Assert.Equal(0, refundResult.RefundAmount);
        Assert.Equal("Transaction not found", refundResult.ErrorMessage);
    }
} 