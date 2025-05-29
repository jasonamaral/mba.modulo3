using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using FluencyHub.PaymentProcessing.Application.Commands.ProcessPayment;
using AppModels = FluencyHub.PaymentProcessing.Application.Common.Models;
using DomainEntities = FluencyHub.PaymentProcessing.Domain;
using AppInterfaces = FluencyHub.PaymentProcessing.Application.Common.Interfaces;

namespace FluencyHub.Tests.Unit.PaymentProcessing.Application.Commands;

public class ProcessPaymentCommandHandlerTests
{
    private readonly Mock<AppInterfaces.IPaymentRepository> _mockPaymentRepository;
    private readonly Mock<AppInterfaces.IPaymentGateway> _mockPaymentGateway;
    private readonly Mock<ILogger<ProcessPaymentCommandHandler>> _mockLogger;
    private readonly ProcessPaymentCommandHandler _handler;

    public ProcessPaymentCommandHandlerTests()
    {
        _mockPaymentRepository = new Mock<AppInterfaces.IPaymentRepository>();
        _mockPaymentGateway = new Mock<AppInterfaces.IPaymentGateway>();
        _mockLogger = new Mock<ILogger<ProcessPaymentCommandHandler>>();
        _handler = new ProcessPaymentCommandHandler(
            _mockPaymentRepository.Object, 
            _mockPaymentGateway.Object, 
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldProcessPaymentSuccessfully_WhenValidCommand()
    {
        // Arrange
        var command = new ProcessPaymentCommand
        {
            StudentId = Guid.NewGuid(),
            EnrollmentId = Guid.NewGuid(),
            Amount = 100.00m,
            PaymentMethod = "CreditCard",
            CardNumber = "4111111111111111",
            CardHolderName = "John Doe",
            ExpirationDate = "12/25",
            SecurityCode = "123"
        };

        var paymentResult = AppModels.PaymentResult.Success("TXN123456");

        _mockPaymentGateway
            .Setup(x => x.ProcessPaymentAsync(
                It.IsAny<string>(),
                It.IsAny<decimal>(),
                It.IsAny<AppModels.CardDetails>()))
            .ReturnsAsync(paymentResult);

        _mockPaymentRepository
            .Setup(x => x.AddAsync(It.IsAny<DomainEntities.Payment>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _mockPaymentGateway.Verify(x => x.ProcessPaymentAsync(
            It.IsAny<string>(),
            command.Amount,
            It.Is<AppModels.CardDetails>(cd => 
                cd.CardHolderName == command.CardHolderName &&
                cd.CardNumber == command.CardNumber &&
                cd.Cvv == command.SecurityCode)), Times.Once);
        _mockPaymentRepository.Verify(x => x.AddAsync(It.IsAny<DomainEntities.Payment>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreatePaymentWithSuccessStatus_WhenGatewaySucceeds()
    {
        // Arrange
        var command = new ProcessPaymentCommand
        {
            StudentId = Guid.NewGuid(),
            EnrollmentId = Guid.NewGuid(),
            Amount = 150.00m,
            PaymentMethod = "CreditCard",
            CardNumber = "4111111111111111",
            CardHolderName = "Jane Smith",
            ExpirationDate = "06/26",
            SecurityCode = "456"
        };

        var paymentResult = AppModels.PaymentResult.Success("TXN789012");

        _mockPaymentGateway
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<AppModels.CardDetails>()))
            .ReturnsAsync(paymentResult);

        DomainEntities.Payment capturedPayment = null!;
        _mockPaymentRepository
            .Setup(x => x.AddAsync(It.IsAny<DomainEntities.Payment>()))
            .Callback<DomainEntities.Payment>((payment) => capturedPayment = payment)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedPayment.Should().NotBeNull();
        capturedPayment.Status.Should().Be(DomainEntities.StatusPagamento.Aprovado);
        capturedPayment.TransactionId.Should().Be("TXN789012");
        capturedPayment.StudentId.Should().Be(command.StudentId);
        capturedPayment.EnrollmentId.Should().Be(command.EnrollmentId);
        capturedPayment.Amount.Should().Be(command.Amount);
        capturedPayment.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldCreatePaymentWithFailedStatus_WhenGatewayFails()
    {
        // Arrange
        var command = new ProcessPaymentCommand
        {
            StudentId = Guid.NewGuid(),
            EnrollmentId = Guid.NewGuid(),
            Amount = 200.00m,
            PaymentMethod = "CreditCard",
            CardNumber = "4111111111111111",
            CardHolderName = "Bob Johnson",
            ExpirationDate = "03/27",
            SecurityCode = "789"
        };

        var paymentResult = AppModels.PaymentResult.Failure("Payment declined");

        _mockPaymentGateway
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<AppModels.CardDetails>()))
            .ReturnsAsync(paymentResult);

        DomainEntities.Payment capturedPayment = null!;
        _mockPaymentRepository
            .Setup(x => x.AddAsync(It.IsAny<DomainEntities.Payment>()))
            .Callback<DomainEntities.Payment>((payment) => capturedPayment = payment)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedPayment.Should().NotBeNull();
        capturedPayment.Status.Should().Be(DomainEntities.StatusPagamento.Falha);
        capturedPayment.TransactionId.Should().BeNull();
        capturedPayment.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenGatewayThrows()
    {
        // Arrange
        var command = new ProcessPaymentCommand
        {
            StudentId = Guid.NewGuid(),
            EnrollmentId = Guid.NewGuid(),
            Amount = 75.00m,
            PaymentMethod = "CreditCard",
            CardNumber = "4111111111111111",
            CardHolderName = "Alice Brown",
            ExpirationDate = "09/28",
            SecurityCode = "321"
        };

        _mockPaymentGateway
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<AppModels.CardDetails>()))
            .ThrowsAsync(new InvalidOperationException("Gateway error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));

        _mockPaymentRepository.Verify(x => x.AddAsync(It.IsAny<DomainEntities.Payment>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldMaskCardNumber_WhenProcessingPayment()
    {
        // Arrange
        var command = new ProcessPaymentCommand
        {
            StudentId = Guid.NewGuid(),
            EnrollmentId = Guid.NewGuid(),
            Amount = 50.00m,
            PaymentMethod = "CreditCard",
            CardNumber = "4111111111111111",
            CardHolderName = "Test User",
            ExpirationDate = "12/25",
            SecurityCode = "123"
        };

        var paymentResult = AppModels.PaymentResult.Success("TXN123");

        _mockPaymentGateway
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<AppModels.CardDetails>()))
            .ReturnsAsync(paymentResult);

        AppModels.CardDetails capturedCardDetails = null!;
        _mockPaymentGateway
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<AppModels.CardDetails>()))
            .Callback<string, decimal, AppModels.CardDetails>((_, _, cardDetails) => capturedCardDetails = cardDetails)
            .ReturnsAsync(paymentResult);

        _mockPaymentRepository
            .Setup(x => x.AddAsync(It.IsAny<DomainEntities.Payment>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedCardDetails.Should().NotBeNull();
        capturedCardDetails.CardNumber.Should().Be("4111111111111111");
        capturedCardDetails.CardHolderName.Should().Be("Test User");
    }

    [Fact]
    public async Task Handle_ShouldLogInformation_WhenProcessingPayment()
    {
        // Arrange
        var command = new ProcessPaymentCommand
        {
            StudentId = Guid.NewGuid(),
            EnrollmentId = Guid.NewGuid(),
            Amount = 100.00m,
            PaymentMethod = "CreditCard",
            CardNumber = "4111111111111111",
            CardHolderName = "John Doe",
            ExpirationDate = "12/25",
            SecurityCode = "123"
        };

        var paymentResult = AppModels.PaymentResult.Success("TXN123456");

        _mockPaymentGateway
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<AppModels.CardDetails>()))
            .ReturnsAsync(paymentResult);

        _mockPaymentRepository
            .Setup(x => x.AddAsync(It.IsAny<DomainEntities.Payment>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing payment for student")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        var command = new ProcessPaymentCommand
        {
            StudentId = Guid.NewGuid(),
            EnrollmentId = Guid.NewGuid(),
            Amount = 100.00m,
            PaymentMethod = "CreditCard",
            CardNumber = "4111111111111111",
            CardHolderName = "John Doe",
            ExpirationDate = "12/25",
            SecurityCode = "123"
        };

        _mockPaymentGateway
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<AppModels.CardDetails>()))
            .ThrowsAsync(new InvalidOperationException("Gateway error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao processar pagamento para estudante")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
} 