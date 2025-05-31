using FluentAssertions;
using FluencyHub.PaymentProcessing.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ApplicationInterfaces = FluencyHub.PaymentProcessing.Application.Common.Interfaces;
using ApplicationModels = FluencyHub.PaymentProcessing.Application.Common.Models;
using DomainModels = FluencyHub.PaymentProcessing.Domain;

namespace FluencyHub.Tests.Unit.PaymentProcessing.Infrastructure.Services;

public class MockPaymentGatewayTests
{
    private readonly Mock<ILogger<MockPaymentGateway>> _mockLogger;
    private readonly Mock<ApplicationInterfaces.IPaymentRepository> _mockPaymentRepository;
    private readonly MockPaymentGateway _gateway;

    public MockPaymentGatewayTests()
    {
        _mockLogger = new Mock<ILogger<MockPaymentGateway>>();
        _mockPaymentRepository = new Mock<ApplicationInterfaces.IPaymentRepository>();
        _gateway = new MockPaymentGateway(_mockLogger.Object, _mockPaymentRepository.Object);
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithValidCard_ShouldReturnSuccess()
    {
        // Arrange
        var orderId = Guid.NewGuid().ToString();
        var amount = 100.00m;
        var cardDetails = new ApplicationModels.CardDetails
        {
            CardHolderName = "João Silva",
            CardNumber = "4111111111111111",
            MaskedCardNumber = "4111****1111",
            ExpiryMonth = "12",
            ExpiryYear = "25",
            Cvv = "123"
        };

        // Act
        var result = await _gateway.ProcessPaymentAsync(orderId, amount, cardDetails);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();
        result.TransactionId.Should().NotBeNullOrEmpty();
        result.TransactionId.Should().StartWith("mock_txn_");
    }

    [Fact]
    public async Task ProcessRefundAsync_WithValidTransactionFromMemory_ShouldReturnSuccess()
    {
        // Arrange
        var orderId = Guid.NewGuid().ToString();
        var amount = 100.00m;
        var cardDetails = new ApplicationModels.CardDetails
        {
            CardHolderName = "João Silva",
            CardNumber = "4111111111111111",
            MaskedCardNumber = "4111****1111",
            ExpiryMonth = "12",
            ExpiryYear = "25",
            Cvv = "123"
        };

        // Primeiro, processar um pagamento
        var paymentResult = await _gateway.ProcessPaymentAsync(orderId, amount, cardDetails);
        var transactionId = paymentResult.TransactionId!;

        // Act
        var refundResult = await _gateway.ProcessRefundAsync(transactionId, amount, "Teste de reembolso");

        // Assert
        refundResult.Should().NotBeNull();
        refundResult.IsSuccessful.Should().BeTrue();
        refundResult.OriginalTransactionId.Should().Be(transactionId);
        refundResult.RefundTransactionId.Should().NotBeNullOrEmpty();
        refundResult.RefundTransactionId.Should().StartWith("mock_refund_");
        refundResult.RefundAmount.Should().Be(amount);
    }

    [Fact]
    public async Task ProcessRefundAsync_WithValidTransactionFromDatabase_ShouldReturnSuccess()
    {
        // Arrange
        var transactionId = "mock_txn_123456789";
        var amount = 100.00m;
        var reason = "Teste de reembolso";

        var payment = new DomainModels.Payment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            amount,
            new DomainModels.CardDetails("João Silva", "4111111111111111", "12", "25"));
        
        payment.MarkAsSuccess(transactionId);

        _mockPaymentRepository
            .Setup(x => x.GetByTransactionIdAsync(transactionId))
            .ReturnsAsync(new List<DomainModels.Payment> { payment });

        // Act
        var refundResult = await _gateway.ProcessRefundAsync(transactionId, amount, reason);

        // Assert
        refundResult.Should().NotBeNull();
        refundResult.IsSuccessful.Should().BeTrue();
        refundResult.OriginalTransactionId.Should().Be(transactionId);
        refundResult.RefundTransactionId.Should().NotBeNullOrEmpty();
        refundResult.RefundTransactionId.Should().StartWith("mock_refund_");
        refundResult.RefundAmount.Should().Be(amount);
    }

    [Fact]
    public async Task ProcessRefundAsync_WithInvalidTransaction_ShouldReturnFailure()
    {
        // Arrange
        var transactionId = "invalid_transaction_id";
        var amount = 100.00m;
        var reason = "Teste de reembolso";

        _mockPaymentRepository
            .Setup(x => x.GetByTransactionIdAsync(transactionId))
            .ReturnsAsync(new List<DomainModels.Payment>());

        // Act
        var refundResult = await _gateway.ProcessRefundAsync(transactionId, amount, reason);

        // Assert
        refundResult.Should().NotBeNull();
        refundResult.IsSuccessful.Should().BeFalse();
        refundResult.OriginalTransactionId.Should().Be(transactionId);
        refundResult.ErrorMessage.Should().Be("Transação não encontrada");
    }

    [Fact]
    public async Task ProcessRefundAsync_WithNonApprovedPayment_ShouldReturnFailure()
    {
        // Arrange
        var transactionId = "mock_txn_123456789";
        var amount = 100.00m;
        var reason = "Teste de reembolso";

        var payment = new DomainModels.Payment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            amount,
            new DomainModels.CardDetails("João Silva", "4111111111111111", "12", "25"));
        
        // Não marcar como sucesso, deixar como pendente

        _mockPaymentRepository
            .Setup(x => x.GetByTransactionIdAsync(transactionId))
            .ReturnsAsync(new List<DomainModels.Payment> { payment });

        // Act
        var refundResult = await _gateway.ProcessRefundAsync(transactionId, amount, reason);

        // Assert
        refundResult.Should().NotBeNull();
        refundResult.IsSuccessful.Should().BeFalse();
        refundResult.OriginalTransactionId.Should().Be(transactionId);
        refundResult.ErrorMessage.Should().Contain("Pagamento não está em status aprovado");
    }
} 