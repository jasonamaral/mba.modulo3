using FluencyHub.Domain.PaymentProcessing;

namespace FluencyHub.Tests.Domain.PaymentProcessing;

public class PaymentTests
{
    [Fact]
    public void Create_Payment_WithValidData_ShouldSucceed()
    {
        // Arrange
        Guid studentId = Guid.NewGuid();
        Guid enrollmentId = Guid.NewGuid();
        decimal amount = 99.99m;
        var cardDetails = CreateValidCardDetails();

        // Act
        var payment = new Payment(studentId, enrollmentId, amount, cardDetails);

        // Assert
        Assert.Equal(studentId, payment.StudentId);
        Assert.Equal(enrollmentId, payment.EnrollmentId);
        Assert.Equal(amount, payment.Amount);
        Assert.Equal(cardDetails, payment.CardDetails);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.True(payment.IsPending);
        Assert.False(payment.IsSuccessful);
        Assert.False(payment.IsFailed);
        Assert.False(payment.IsRefunded);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_Payment_WithInvalidAmount_ShouldThrowException(decimal amount)
    {
        // Arrange
        Guid studentId = Guid.NewGuid();
        Guid enrollmentId = Guid.NewGuid();
        var cardDetails = CreateValidCardDetails();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Payment(studentId, enrollmentId, amount, cardDetails));
        Assert.Contains("amount", exception.Message.ToLower());
    }

    [Fact]
    public void Create_Payment_WithNullCardDetails_ShouldThrowException()
    {
        // Arrange
        Guid studentId = Guid.NewGuid();
        Guid enrollmentId = Guid.NewGuid();
        decimal amount = 99.99m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Payment(studentId, enrollmentId, amount, null));
        Assert.Contains("card details", exception.Message.ToLower());
    }

    [Fact]
    public void MarkAsSuccess_PendingPayment_ShouldUpdateStatusAndTransactionId()
    {
        // Arrange
        var payment = CreateValidPayment();
        string transactionId = "txn_123456789";

        // Act
        payment.MarkAsSuccess(transactionId);

        // Assert
        Assert.Equal(PaymentStatus.Successful, payment.Status);
        Assert.Equal(transactionId, payment.TransactionId);
        Assert.True(payment.IsSuccessful);
        Assert.False(payment.IsPending);
    }

    [Fact]
    public void MarkAsSuccess_NonPendingPayment_ShouldThrowException()
    {
        // Arrange
        var payment = CreateValidPayment();
        payment.MarkAsSuccess("txn_123456789");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            payment.MarkAsSuccess("txn_987654321"));
    }

    [Fact]
    public void MarkAsSuccess_WithEmptyTransactionId_ShouldThrowException()
    {
        // Arrange
        var payment = CreateValidPayment();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            payment.MarkAsSuccess(""));
        Assert.Contains("transaction id", exception.Message.ToLower());
    }

    [Fact]
    public void MarkAsFailed_PendingPayment_ShouldUpdateStatus()
    {
        // Arrange
        var payment = CreateValidPayment();
        string reason = "Payment rejected by issuer";

        // Act
        payment.MarkAsFailed(reason);

        // Assert
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.True(payment.IsFailed);
        Assert.False(payment.IsPending);
    }

    [Fact]
    public void MarkAsFailed_NonPendingPayment_ShouldThrowException()
    {
        // Arrange
        var payment = CreateValidPayment();
        payment.MarkAsSuccess("txn_123456789");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            payment.MarkAsFailed("Failed after success"));
    }

    [Fact]
    public void MarkAsRefunded_SuccessfulPayment_ShouldUpdateStatus()
    {
        // Arrange
        var payment = CreateValidPayment();
        payment.MarkAsSuccess("txn_123456789");
        string reason = "Customer requested refund";

        // Act
        payment.MarkAsRefunded(reason);

        // Assert
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.True(payment.IsRefunded);
        Assert.False(payment.IsSuccessful);
    }

    [Fact]
    public void MarkAsRefunded_NonSuccessfulPayment_ShouldThrowException()
    {
        // Arrange
        var payment = CreateValidPayment();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            payment.MarkAsRefunded("Refund without success"));
    }

    private CardDetails CreateValidCardDetails()
    {
        return new CardDetails(
            "John Doe",
            "4111111111111111",
            "12",
            "2030"
        );
    }

    private Payment CreateValidPayment()
    {
        return new Payment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            99.99m,
            CreateValidCardDetails()
        );
    }
} 