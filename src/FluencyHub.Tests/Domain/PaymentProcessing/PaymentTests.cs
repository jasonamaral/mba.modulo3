using FluencyHub.Domain.PaymentProcessing;
using System;
using Xunit;

namespace FluencyHub.Tests.Domain.PaymentProcessing;

public class PaymentTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var cardDetails = new CardDetails("John Doe", "4111111111111111", "12", "2030");
        var amount = 99.99m;
        
        // Act
        var payment = new Payment(studentId, enrollmentId, amount, cardDetails);
        
        // Assert
        Assert.Equal(studentId, payment.StudentId);
        Assert.Equal(enrollmentId, payment.EnrollmentId);
        Assert.Equal(amount, payment.Amount);
        Assert.Equal(cardDetails, payment.CardDetails);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Null(payment.TransactionId);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidAmount_ThrowsArgumentException(decimal amount)
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var cardDetails = new CardDetails("John Doe", "4111111111111111", "12", "2030");
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new Payment(studentId, enrollmentId, amount, cardDetails));
        Assert.Contains("Payment amount must be positive", exception.Message);
    }
    
    [Fact]
    public void Constructor_WithNullCardDetails_ThrowsArgumentException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var amount = 99.99m;
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new Payment(studentId, enrollmentId, amount, null));
        Assert.Contains("Card details cannot be null", exception.Message);
    }
    
    [Fact]
    public void MarkAsSuccess_WithValidTransaction_UpdatesStatusAndTransactionId()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var cardDetails = new CardDetails("John Doe", "4111111111111111", "12", "2030");
        var amount = 99.99m;
        var payment = new Payment(studentId, enrollmentId, amount, cardDetails);
        var transactionId = "TXN-123456";
        
        // Act
        payment.MarkAsSuccess(transactionId);
        
        // Assert
        Assert.Equal(PaymentStatus.Successful, payment.Status);
        Assert.Equal(transactionId, payment.TransactionId);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void MarkAsSuccess_WithInvalidTransactionId_ThrowsArgumentException(string transactionId)
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var cardDetails = new CardDetails("John Doe", "4111111111111111", "12", "2030");
        var amount = 99.99m;
        var payment = new Payment(studentId, enrollmentId, amount, cardDetails);
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => payment.MarkAsSuccess(transactionId));
        Assert.Contains("Transaction ID cannot be empty", exception.Message);
    }
    
    [Fact]
    public void MarkAsSuccess_WhenAlreadyCompleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var cardDetails = new CardDetails("John Doe", "4111111111111111", "12", "2030");
        var amount = 99.99m;
        var payment = new Payment(studentId, enrollmentId, amount, cardDetails);
        payment.MarkAsSuccess("TXN-123456");
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => payment.MarkAsSuccess("TXN-789012"));
        Assert.Contains("Cannot mark as success a payment with status", exception.Message);
    }
    
    [Fact]
    public void MarkAsFailed_UpdatesStatusAndReason()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var cardDetails = new CardDetails("John Doe", "4111111111111111", "12", "2030");
        var amount = 99.99m;
        var payment = new Payment(studentId, enrollmentId, amount, cardDetails);
        var reason = "Insufficient funds";
        
        // Act
        payment.MarkAsFailed(reason);
        
        // Assert
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal(reason, payment.FailureReason);
    }
    
    [Fact]
    public void MarkAsFailed_WhenAlreadyCompleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var cardDetails = new CardDetails("John Doe", "4111111111111111", "12", "2030");
        var amount = 99.99m;
        var payment = new Payment(studentId, enrollmentId, amount, cardDetails);
        payment.MarkAsSuccess("TXN-123456");
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => payment.MarkAsFailed("Insufficient funds"));
        Assert.Contains("Cannot mark as failed a payment with status", exception.Message);
    }
    
    [Fact]
    public void MarkAsRefunded_WhenSuccessful_UpdatesStatusAndRefundReason()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var cardDetails = new CardDetails("John Doe", "4111111111111111", "12", "2030");
        var amount = 99.99m;
        var payment = new Payment(studentId, enrollmentId, amount, cardDetails);
        payment.MarkAsSuccess("TXN-123456");
        var refundReason = "Customer request";
        
        // Act
        payment.MarkAsRefunded(refundReason);
        
        // Assert
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Equal(refundReason, payment.RefundReason);
    }
    
    [Fact]
    public void MarkAsRefunded_WhenNotSuccessful_ThrowsInvalidOperationException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var cardDetails = new CardDetails("John Doe", "4111111111111111", "12", "2030");
        var amount = 99.99m;
        var payment = new Payment(studentId, enrollmentId, amount, cardDetails);
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => payment.MarkAsRefunded("Customer request"));
        Assert.Contains("Only successful payments can be refunded", exception.Message);
    }
    
    [Fact]
    public void IsSuccessful_ReturnsTrueForSuccessfulPayment()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var cardDetails = new CardDetails("John Doe", "4111111111111111", "12", "2030");
        var amount = 99.99m;
        var payment = new Payment(studentId, enrollmentId, amount, cardDetails);
        payment.MarkAsSuccess("TXN-123456");
        
        // Assert
        Assert.True(payment.IsSuccessful);
        Assert.False(payment.IsFailed);
        Assert.False(payment.IsPending);
        Assert.False(payment.IsRefunded);
    }
    
    [Fact]
    public void IsFailed_ReturnsTrueForFailedPayment()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var cardDetails = new CardDetails("John Doe", "4111111111111111", "12", "2030");
        var amount = 99.99m;
        var payment = new Payment(studentId, enrollmentId, amount, cardDetails);
        payment.MarkAsFailed("Insufficient funds");
        
        // Assert
        Assert.False(payment.IsSuccessful);
        Assert.True(payment.IsFailed);
        Assert.False(payment.IsPending);
        Assert.False(payment.IsRefunded);
    }
    
    [Fact]
    public void IsRefunded_ReturnsTrueForRefundedPayment()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var cardDetails = new CardDetails("John Doe", "4111111111111111", "12", "2030");
        var amount = 99.99m;
        var payment = new Payment(studentId, enrollmentId, amount, cardDetails);
        payment.MarkAsSuccess("TXN-123456");
        payment.MarkAsRefunded("Customer request");
        
        // Assert
        Assert.False(payment.IsSuccessful);
        Assert.False(payment.IsFailed);
        Assert.False(payment.IsPending);
        Assert.True(payment.IsRefunded);
    }
} 