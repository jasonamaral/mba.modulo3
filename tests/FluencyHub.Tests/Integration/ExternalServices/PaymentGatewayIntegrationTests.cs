using FluencyHub.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using FluencyHub.PaymentProcessing.Application.Common.Interfaces;
using FluencyHub.PaymentProcessing.Domain;
using Xunit;

namespace FluencyHub.Tests.Integration.ExternalServices;

public class PaymentGatewayIntegrationTests : IntegrationTestBase
{
    // 177. PaymentGateway_RefundPaymentAsync_ShouldReturnSuccess_WhenValidRefund
    [Fact]
    public async Task PaymentGateway_RefundPaymentAsync_ShouldReturnSuccess_WhenValidRefund()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var paymentGateway = scope.ServiceProvider.GetRequiredService<FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentGateway>();
        
        var transactionId = "TXN123456789";
        var refundAmount = 150.00m;
        var reason = "Solicitação do cliente";

        // Act
        var result = await paymentGateway.ProcessRefundAsync(transactionId, refundAmount, reason);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();
        result.RefundTransactionId.Should().NotBeNullOrEmpty();
        result.RefundAmount.Should().Be(refundAmount);
        result.OriginalTransactionId.Should().Be(transactionId);
    }

    // 221. PaymentGateway_Integration_ShouldProcessRealPayment_WhenValidData
    [Fact]
    public async Task PaymentGateway_Integration_ShouldProcessRealPayment_WhenValidData()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var paymentGateway = scope.ServiceProvider.GetRequiredService<FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentGateway>();
        
        var validCardDetails = TestDataBuilder.CreateValidApplicationCardDetails(
            cardholderName: "João Silva",
            cardNumber: "4532015112830366", // Número de teste válido
            expiryMonth: "12",
            expiryYear: "2025"
        );
        
        var amount = 299.99m;

        // Act
        var result = await paymentGateway.ProcessPaymentAsync("ORDER123", amount, validCardDetails);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();
        result.TransactionId.Should().NotBeNullOrEmpty();
    }

    // 222. PaymentGateway_Integration_ShouldHandleTimeout_WhenGatewayUnavailable
    [Fact]
    public async Task PaymentGateway_Integration_ShouldHandleTimeout_WhenGatewayUnavailable()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var paymentGateway = scope.ServiceProvider.GetRequiredService<FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentGateway>();
        
        var validCardDetails = TestDataBuilder.CreateValidApplicationCardDetails();
        var amount = 299.99m;

        // Act - Usar orderId que simula timeout
        var result = await paymentGateway.ProcessPaymentAsync("ORDER_timeout_123", amount, validCardDetails);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessage.Should().Contain("timeout");
    }

    // 223. PaymentGateway_Integration_ShouldReturnError_WhenInvalidCardData
    [Fact]
    public async Task PaymentGateway_Integration_ShouldReturnError_WhenInvalidCardData()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var paymentGateway = scope.ServiceProvider.GetRequiredService<FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentGateway>();
        
        var invalidCardDetails = TestDataBuilder.CreateValidApplicationCardDetails(
            cardholderName: "João Silva",
            cardNumber: "1234567890123456", // Número inválido
            expiryMonth: "12",
            expiryYear: "2025"
        );
        
        var amount = 299.99m;

        // Act
        var result = await paymentGateway.ProcessPaymentAsync("ORDER123", amount, invalidCardDetails);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.ErrorMessage.Should().Contain("recusado");
    }

    // Teste adicional: Verificar se o gateway lida com cartões expirados
    [Fact]
    public async Task PaymentGateway_Integration_ShouldReturnError_WhenCardExpired()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var paymentGateway = scope.ServiceProvider.GetRequiredService<FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentGateway>();
        
        var expiredCardDetails = TestDataBuilder.CreateValidApplicationCardDetails(
            cardholderName: "João Silva",
            cardNumber: "4532015112830366",
            expiryMonth: "01",
            expiryYear: "2020" // Cartão expirado
        );
        
        var amount = 299.99m;

        // Act
        var result = await paymentGateway.ProcessPaymentAsync("ORDER123", amount, expiredCardDetails);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.ErrorMessage.Should().Contain("recusado");
    }

    // Teste adicional: Verificar se o gateway lida com valores inválidos
    [Fact]
    public async Task PaymentGateway_Integration_ShouldReturnError_WhenAmountIsZeroOrNegative()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var paymentGateway = scope.ServiceProvider.GetRequiredService<FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentGateway>();
        
        var validCardDetails = TestDataBuilder.CreateValidApplicationCardDetails();

        // Act & Assert - Valor zero
        var zeroResult = await paymentGateway.ProcessPaymentAsync("ORDER123", 0m, validCardDetails);
        zeroResult.Should().NotBeNull();
        zeroResult.IsSuccessful.Should().BeFalse();

        // Act & Assert - Valor negativo
        var negativeResult = await paymentGateway.ProcessPaymentAsync("ORDER123", -100m, validCardDetails);
        negativeResult.Should().NotBeNull();
        negativeResult.IsSuccessful.Should().BeFalse();
    }
} 