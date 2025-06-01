using FluentAssertions;
using FluencyHub.PaymentProcessing.Domain;
using FluencyHub.PaymentProcessing.Domain.Events;
using Xunit;

namespace FluencyHub.Tests.Unit.PaymentProcessing.Domain;

public class PaymentTests
{
    private CardDetails CreateValidCardDetails()
    {
        return new CardDetails("João Silva", "4532015112830366", "12", "25");
    }

    [Fact]
    public void Payment_Constructor_ShouldCreateValidPayment()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var amount = 299.99m;
        var cardDetails = CreateValidCardDetails();

        // Act
        var payment = new Payment(studentId, enrollmentId, amount, cardDetails);

        // Assert
        payment.StudentId.Should().Be(studentId);
        payment.EnrollmentId.Should().Be(enrollmentId);
        payment.Amount.Should().Be(amount);
        payment.CardDetails.Should().Be(cardDetails);
        payment.Status.Should().Be(StatusPagamento.Pendente);
        payment.PaymentDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        payment.TransactionId.Should().BeNull();
        payment.FailureReason.Should().BeNull();
        payment.RefundReason.Should().BeNull();
        payment.IsPending.Should().BeTrue();
        payment.IsSuccessful.Should().BeFalse();
        payment.IsFailed.Should().BeFalse();
        payment.IsRefunded.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-0.01)]
    public void Payment_Constructor_WithInvalidAmount_ShouldThrowArgumentException(decimal invalidAmount)
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var cardDetails = CreateValidCardDetails();

        // Act & Assert
        var action = () => new Payment(studentId, enrollmentId, invalidAmount, cardDetails);
        action.Should().Throw<ArgumentException>()
            .WithMessage("O valor do pagamento deve ser positivo*");
    }

    [Fact]
    public void Payment_Constructor_WithNullCardDetails_ShouldThrowArgumentException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var amount = 100m;
        CardDetails cardDetails = null!;

        // Act & Assert
        var action = () => new Payment(studentId, enrollmentId, amount, cardDetails);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Os detalhes do cartão não podem ser nulos*");
    }

    [Fact]
    public void MarkAsSuccess_WithValidTransactionId_ShouldMarkPaymentAsSuccessful()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Guid.NewGuid(), 100m, CreateValidCardDetails());
        var transactionId = "TXN123456789";

        // Act
        payment.MarkAsSuccess(transactionId);

        // Assert
        payment.TransactionId.Should().Be(transactionId);
        payment.Status.Should().Be(StatusPagamento.Aprovado);
        payment.IsSuccessful.Should().BeTrue();
        payment.IsPending.Should().BeFalse();
        payment.IsFailed.Should().BeFalse();
        payment.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsSuccess_WithValidTransactionId_ShouldAddPaymentConfirmedDomainEvent()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var payment = new Payment(Guid.NewGuid(), enrollmentId, 100m, CreateValidCardDetails());
        var transactionId = "TXN123456789";

        // Act
        payment.MarkAsSuccess(transactionId);

        // Assert
        payment.DomainEvents.Should().HaveCount(1);
        var domainEvent = payment.DomainEvents.First() as PaymentConfirmedDomainEvent;
        domainEvent.Should().NotBeNull();
        domainEvent!.PaymentId.Should().Be(payment.Id);
        domainEvent.EnrollmentId.Should().Be(enrollmentId);
        domainEvent.TransactionId.Should().Be(transactionId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void MarkAsSuccess_WithInvalidTransactionId_ShouldThrowArgumentException(string? invalidTransactionId)
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Guid.NewGuid(), 100m, CreateValidCardDetails());

        // Act & Assert
        var action = () => payment.MarkAsSuccess(invalidTransactionId!);
        action.Should().Throw<ArgumentException>()
            .WithMessage("O ID da transação não pode estar vazio*");
    }

    [Fact]
    public void MarkAsSuccess_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Guid.NewGuid(), 100m, CreateValidCardDetails());
        payment.MarkAsSuccess("TXN123");

        // Act & Assert
        var action = () => payment.MarkAsSuccess("TXN456");
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Não é possível marcar como bem-sucedido um pagamento com status Aprovado");
    }

    [Fact]
    public void MarkAsFailed_WithValidReason_ShouldMarkPaymentAsFailed()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Guid.NewGuid(), 100m, CreateValidCardDetails());
        var failureReason = "Cartão recusado";

        // Act
        payment.MarkAsFailed(failureReason);

        // Assert
        payment.FailureReason.Should().Be(failureReason);
        payment.Status.Should().Be(StatusPagamento.Falha);
        payment.IsFailed.Should().BeTrue();
        payment.IsPending.Should().BeFalse();
        payment.IsSuccessful.Should().BeFalse();
        payment.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void MarkAsFailed_WithInvalidReason_ShouldThrowArgumentException(string? invalidReason)
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Guid.NewGuid(), 100m, CreateValidCardDetails());

        // Act & Assert
        var action = () => payment.MarkAsFailed(invalidReason!);
        action.Should().Throw<ArgumentException>()
            .WithMessage("A razão da falha não pode estar vazia*");
    }

    [Fact]
    public void MarkAsFailed_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Guid.NewGuid(), 100m, CreateValidCardDetails());
        payment.MarkAsSuccess("TXN123");

        // Act & Assert
        var action = () => payment.MarkAsFailed("Erro");
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Não é possível marcar como falho um pagamento com status Aprovado");
    }

    [Fact]
    public void MarkAsRefunded_WithValidReason_ShouldMarkPaymentAsRefunded()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Guid.NewGuid(), 100m, CreateValidCardDetails());
        payment.MarkAsSuccess("TXN123");
        var refundReason = "Solicitação do cliente";

        // Act
        payment.MarkAsRefunded(refundReason);

        // Assert
        payment.RefundReason.Should().Be(refundReason);
        payment.Status.Should().Be(StatusPagamento.Reembolsado);
        payment.IsRefunded.Should().BeTrue();
        payment.IsSuccessful.Should().BeFalse();
        payment.IsPending.Should().BeFalse();
        payment.IsFailed.Should().BeFalse();
        payment.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void MarkAsRefunded_WithInvalidReason_ShouldThrowArgumentException(string? invalidReason)
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Guid.NewGuid(), 100m, CreateValidCardDetails());
        payment.MarkAsSuccess("TXN123");

        // Act & Assert
        var action = () => payment.MarkAsRefunded(invalidReason!);
        action.Should().Throw<ArgumentException>()
            .WithMessage("A razão do reembolso não pode estar vazia*");
    }

    [Fact]
    public void MarkAsRefunded_WhenNotApproved_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Guid.NewGuid(), 100m, CreateValidCardDetails());

        // Act & Assert
        var action = () => payment.MarkAsRefunded("Reembolso");
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Não é possível reembolsar um pagamento com status Pendente");
    }

    [Fact]
    public void Payment_StatusProperties_ShouldReflectCurrentStatus()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Guid.NewGuid(), 100m, CreateValidCardDetails());

        // Act & Assert - Pendente
        payment.IsPending.Should().BeTrue();
        payment.IsSuccessful.Should().BeFalse();
        payment.IsFailed.Should().BeFalse();
        payment.IsRefunded.Should().BeFalse();

        // Act - Marcar como sucesso
        payment.MarkAsSuccess("TXN123");

        // Assert - Aprovado
        payment.IsPending.Should().BeFalse();
        payment.IsSuccessful.Should().BeTrue();
        payment.IsFailed.Should().BeFalse();
        payment.IsRefunded.Should().BeFalse();

        // Act - Marcar como reembolsado
        payment.MarkAsRefunded("Reembolso");

        // Assert - Reembolsado
        payment.IsPending.Should().BeFalse();
        payment.IsSuccessful.Should().BeFalse();
        payment.IsFailed.Should().BeFalse();
        payment.IsRefunded.Should().BeTrue();
    }

    [Fact]
    public void Payment_FailedStatus_ShouldReflectCorrectly()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Guid.NewGuid(), 100m, CreateValidCardDetails());

        // Act
        payment.MarkAsFailed("Cartão inválido");

        // Assert
        payment.IsPending.Should().BeFalse();
        payment.IsSuccessful.Should().BeFalse();
        payment.IsFailed.Should().BeTrue();
        payment.IsRefunded.Should().BeFalse();
    }
} 