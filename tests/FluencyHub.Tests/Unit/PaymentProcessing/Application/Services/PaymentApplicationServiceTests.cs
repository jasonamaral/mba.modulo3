using FluencyHub.PaymentProcessing.Application.Common.Interfaces;
using FluencyHub.PaymentProcessing.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace FluencyHub.Tests.Unit.PaymentProcessing.Application.Services;

public class PaymentApplicationServiceTests
{
    private readonly Mock<FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentRepository> _mockPaymentRepository;
    private readonly Mock<FluencyHub.PaymentProcessing.Application.Common.Interfaces.IEnrollmentRepository> _mockEnrollmentRepository;
    private readonly Mock<FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentGateway> _mockPaymentGateway;
    private readonly Mock<FluencyHub.SharedKernel.Events.IDomainEventService> _mockEventService;
    private readonly Mock<ILogger<FluencyHub.PaymentProcessing.Application.Services.PaymentService>> _mockLogger;
    private readonly FluencyHub.PaymentProcessing.Application.Services.PaymentService _service;

    public PaymentApplicationServiceTests()
    {
        _mockPaymentRepository = new Mock<FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentRepository>();
        _mockEnrollmentRepository = new Mock<FluencyHub.PaymentProcessing.Application.Common.Interfaces.IEnrollmentRepository>();
        _mockPaymentGateway = new Mock<FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentGateway>();
        _mockEventService = new Mock<FluencyHub.SharedKernel.Events.IDomainEventService>();
        _mockLogger = new Mock<ILogger<FluencyHub.PaymentProcessing.Application.Services.PaymentService>>();
        _service = new FluencyHub.PaymentProcessing.Application.Services.PaymentService(
            _mockPaymentRepository.Object,
            _mockEnrollmentRepository.Object,
            _mockPaymentGateway.Object,
            _mockEventService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithValidData_ShouldProcessPaymentSuccessfully()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var cardNumber = "4532015112830366";
        var cardHolderName = "João Silva";
        var expiryMonth = "12";
        var expiryYear = "2025";

        var mockEnrollment = new Mock<FluencyHub.SharedKernel.Contracts.IEnrollment>();
        mockEnrollment.Setup(x => x.Id).Returns(enrollmentId);
        mockEnrollment.Setup(x => x.StudentId).Returns(studentId);
        mockEnrollment.Setup(x => x.Price).Returns(299.99m);
        mockEnrollment.Setup(x => x.Status).Returns("Pendente");

        _mockEnrollmentRepository
            .Setup(x => x.GetByIdAsync(enrollmentId))
            .ReturnsAsync(mockEnrollment.Object);

        var paymentResult = FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult.Success("TXN123456789");

        _mockPaymentGateway
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<FluencyHub.PaymentProcessing.Application.Common.Models.CardDetails>()))
            .ReturnsAsync(paymentResult);

        _mockPaymentRepository
            .Setup(x => x.AddAsync(It.IsAny<Payment>()))
            .Returns(Task.CompletedTask);

        _mockPaymentRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ProcessPaymentAsync(
            enrollmentId,
            cardHolderName,
            cardNumber,
            expiryMonth,
            expiryYear);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _mockPaymentRepository.Verify(x => x.AddAsync(It.IsAny<Payment>()), Times.Once);
        _mockPaymentRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentAsync_WhenPaymentGatewayFails_ShouldThrowException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var cardNumber = "4532015112830366";
        var cardHolderName = "João Silva";
        var expiryDate = "12/2025";
        var cvv = "123";

        var paymentResult = FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult.Failure("Cartão recusado");

        _mockPaymentGateway
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<FluencyHub.PaymentProcessing.Application.Common.Models.CardDetails>()))
            .ReturnsAsync(paymentResult);

        // Act & Assert
        var action = async () => await _service.ProcessPaymentAsync(
            studentId,
            cardNumber,
            cardHolderName,
            expiryDate,
            cvv);

        await action.Should().ThrowAsync<FluencyHub.PaymentProcessing.Application.Common.Exceptions.PaymentProcessingException>()
            .WithMessage("Cartão recusado");
    }
} 