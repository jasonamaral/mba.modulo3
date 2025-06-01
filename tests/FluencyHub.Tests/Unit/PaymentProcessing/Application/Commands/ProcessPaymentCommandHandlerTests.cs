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
            Amount = 299.99m,
            PaymentMethod = "CreditCard",
            CardHolderName = "João Silva",
            CardNumber = "4532015112830366", // Número válido Visa
            ExpirationDate = "12/25",
            SecurityCode = "123"
        };

        var paymentResult = AppModels.PaymentResult.Success("TXN123456");

        _mockPaymentGateway
            .Setup(x => x.ProcessPaymentAsync(
                It.IsAny<string>(),
                command.Amount,
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
            It.Is<AppModels.CardDetails>(c => 
                c.CardHolderName == command.CardHolderName &&
                c.MaskedCardNumber == "**** **** **** 0366")), Times.Once);
        
        _mockPaymentRepository.Verify(x => x.AddAsync(It.IsAny<DomainEntities.Payment>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMarkPaymentAsFailed_WhenGatewayFails()
    {
        // Arrange
        var command = new ProcessPaymentCommand
        {
            StudentId = Guid.NewGuid(),
            EnrollmentId = Guid.NewGuid(),
            Amount = 199.99m,
            PaymentMethod = "CreditCard",
            CardHolderName = "Maria Santos",
            CardNumber = "5555555555554444", // Número válido Mastercard
            ExpirationDate = "06/26",
            SecurityCode = "456"
        };

        var paymentResult = AppModels.PaymentResult.Failure("Cartão recusado");

        _mockPaymentGateway
            .Setup(x => x.ProcessPaymentAsync(
                It.IsAny<string>(),
                command.Amount,
                It.IsAny<AppModels.CardDetails>()))
            .ReturnsAsync(paymentResult);

        DomainEntities.Payment capturedPayment = null!;
        _mockPaymentRepository
            .Setup(x => x.AddAsync(It.IsAny<DomainEntities.Payment>()))
            .Callback<DomainEntities.Payment>((payment) => capturedPayment = payment)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        capturedPayment.Should().NotBeNull();
        capturedPayment.Status.Should().Be(DomainEntities.StatusPagamento.Falha);
        
        _mockPaymentRepository.Verify(x => x.AddAsync(It.IsAny<DomainEntities.Payment>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        var command = new ProcessPaymentCommand
        {
            StudentId = Guid.NewGuid(),
            EnrollmentId = Guid.NewGuid(),
            Amount = 399.99m,
            PaymentMethod = "CreditCard",
            CardHolderName = "Pedro Costa",
            CardNumber = "378282246310005", // Número válido Amex
            ExpirationDate = "03/27",
            SecurityCode = "789"
        };

        _mockPaymentGateway
            .Setup(x => x.ProcessPaymentAsync(
                It.IsAny<string>(),
                It.IsAny<decimal>(),
                It.IsAny<AppModels.CardDetails>()))
            .ThrowsAsync(new InvalidOperationException("Erro no gateway"));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Erro no gateway");

        // Verify that error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Erro ao processar pagamento para estudante {command.StudentId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMaskCardNumberCorrectly()
    {
        // Arrange
        var command = new ProcessPaymentCommand
        {
            StudentId = Guid.NewGuid(),
            EnrollmentId = Guid.NewGuid(),
            Amount = 149.99m,
            PaymentMethod = "CreditCard",
            CardHolderName = "Ana Oliveira",
            CardNumber = "4000000000000002", // Número válido Visa
            ExpirationDate = "09/26",
            SecurityCode = "321"
        };

        var paymentResult = AppModels.PaymentResult.Success("TXN789012");

        _mockPaymentGateway
            .Setup(x => x.ProcessPaymentAsync(
                It.IsAny<string>(),
                command.Amount,
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
            It.Is<AppModels.CardDetails>(c => c.MaskedCardNumber == "**** **** **** 0002")), Times.Once);
    }
} 