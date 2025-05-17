using System;
using Xunit;
using FluencyHub.PaymentProcessing.Domain;
using FluencyHub.PaymentProcessing.Domain.Events;

namespace FluencyHub.Tests.Domain.PaymentProcessing;

public class PaymentTests
{
    private readonly CardDetails _validCardDetails;
    private readonly Guid _studentId = Guid.NewGuid();
    private readonly Guid _enrollmentId = Guid.NewGuid();
    private readonly decimal _amount = 100.00m;

    public PaymentTests()
    {
        // Criando CardDetails válido para os testes
        _validCardDetails = new CardDetails(
            "John Doe",
            "4111111111111111", // Válido para teste
            "12",
            (DateTime.UtcNow.Year + 1).ToString());
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreatePayment()
    {
        // Act
        var payment = new Payment(_studentId, _enrollmentId, _amount, _validCardDetails);

        // Assert
        Assert.Equal(_studentId, payment.StudentId);
        Assert.Equal(_enrollmentId, payment.EnrollmentId);
        Assert.Equal(_amount, payment.Amount);
        Assert.Equal(_validCardDetails, payment.CardDetails);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.True(DateTime.UtcNow.Subtract(payment.PaymentDate).TotalSeconds < 1);
        Assert.True(DateTime.UtcNow.Subtract(payment.CreatedAt).TotalSeconds < 1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidAmount_ShouldThrowArgumentException(decimal invalidAmount)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Payment(
            _studentId,
            _enrollmentId,
            invalidAmount,
            _validCardDetails));
    }

    [Fact]
    public void Constructor_WithNullCardDetails_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Payment(
            _studentId,
            _enrollmentId,
            _amount,
            null!));
    }

    [Fact]
    public void MarkAsSuccess_ShouldUpdateStatusAndAddDomainEvent()
    {
        // Arrange
        var payment = new Payment(_studentId, _enrollmentId, _amount, _validCardDetails);
        var transactionId = "TRX123456";

        // Act
        payment.MarkAsSuccess(transactionId);

        // Assert
        Assert.Equal(PaymentStatus.Successful, payment.Status);
        Assert.Equal(transactionId, payment.TransactionId);
        Assert.NotNull(payment.UpdatedAt);
        Assert.True(payment.IsSuccessful);
        Assert.False(payment.IsFailed);
        Assert.False(payment.IsPending);
        Assert.False(payment.IsRefunded);

        // Verificar evento de domínio
        Assert.Single(payment.DomainEvents);
        var @event = Assert.IsType<PaymentConfirmedDomainEvent>(payment.DomainEvents.First());
        Assert.Equal(payment.Id, @event.PaymentId);
        Assert.Equal(_enrollmentId, @event.EnrollmentId);
        Assert.Equal(transactionId, @event.TransactionId);
    }

    [Fact]
    public void MarkAsSuccess_WithEmptyTransactionId_ShouldThrowArgumentException()
    {
        // Arrange
        var payment = new Payment(_studentId, _enrollmentId, _amount, _validCardDetails);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => payment.MarkAsSuccess(""));
    }

    [Fact]
    public void MarkAsSuccess_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = new Payment(_studentId, _enrollmentId, _amount, _validCardDetails);
        payment.MarkAsSuccess("TRX123456");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => payment.MarkAsSuccess("TRX789012"));
    }

    [Fact]
    public void MarkAsFailed_ShouldUpdateStatusAndAddDomainEvent()
    {
        // Arrange
        var payment = new Payment(_studentId, _enrollmentId, _amount, _validCardDetails);
        var failureReason = "Insufficient funds";

        // Act
        payment.MarkAsFailed(failureReason);

        // Assert
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal(failureReason, payment.FailureReason);
        Assert.NotNull(payment.UpdatedAt);
        Assert.False(payment.IsSuccessful);
        Assert.True(payment.IsFailed);
        Assert.False(payment.IsPending);
        Assert.False(payment.IsRefunded);

        // Verificar evento de domínio
        Assert.Single(payment.DomainEvents);
        var @event = Assert.IsType<PaymentRejectedDomainEvent>(payment.DomainEvents.First());
        Assert.Equal(payment.Id, @event.PaymentId);
        Assert.Equal(_enrollmentId, @event.EnrollmentId);
        Assert.Equal(failureReason, @event.FailureReason);
    }

    [Fact]
    public void MarkAsFailed_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = new Payment(_studentId, _enrollmentId, _amount, _validCardDetails);
        payment.MarkAsSuccess("TRX123456");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => payment.MarkAsFailed("Payment already succeeded"));
    }

    [Fact]
    public void MarkAsRefunded_ShouldUpdateStatus()
    {
        // Arrange
        var payment = new Payment(_studentId, _enrollmentId, _amount, _validCardDetails);
        payment.MarkAsSuccess("TRX123456");
        var refundReason = "Customer request";

        // Act
        payment.MarkAsRefunded(refundReason);

        // Assert
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Equal(refundReason, payment.RefundReason);
        Assert.NotNull(payment.UpdatedAt);
        Assert.False(payment.IsSuccessful);
        Assert.False(payment.IsFailed);
        Assert.False(payment.IsPending);
        Assert.True(payment.IsRefunded);
    }

    [Fact]
    public void MarkAsRefunded_WhenNotSuccessful_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = new Payment(_studentId, _enrollmentId, _amount, _validCardDetails);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => payment.MarkAsRefunded("Cannot refund pending payment"));
    }
} 