using FluencyHub.API.Controllers;
using FluencyHub.API.Models;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.Common.Exceptions;
using FluencyHub.PaymentProcessing.Domain;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FluencyHub.Tests.API.Controllers;

public class PaymentsControllerTests
{
    private readonly Mock<IPaymentApplicationService> _paymentServiceMock;
    private readonly PaymentsController _controller;

    public PaymentsControllerTests()
    {
        _paymentServiceMock = new Mock<IPaymentApplicationService>();
        _controller = new PaymentsController(_paymentServiceMock.Object);
    }

    [Fact]
    public async Task ProcessPayment_WithValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var request = new PaymentProcessRequest
        {
            EnrollmentId = Guid.NewGuid(),
            Amount = 99.99m,
            CardDetails = new CardDetailsRequest
            {
                CardholderName = "John Doe",
                CardNumber = "4111111111111111",
                ExpiryMonth = 12,
                ExpiryYear = 2030,
                Cvv = "123"
            }
        };

        var paymentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        
        // Create a domain payment object
        var payment = new Payment(
            studentId, 
            request.EnrollmentId, 
            request.Amount, 
            new CardDetails(
                request.CardDetails.CardholderName,
                request.CardDetails.CardNumber,
                request.CardDetails.ExpiryMonth.ToString(),
                request.CardDetails.ExpiryYear.ToString()
            )
        );

        // Set payment ID through reflection
        typeof(Payment).GetProperty("Id").SetValue(payment, paymentId);

        _paymentServiceMock
            .Setup(s => s.ProcessPaymentAsync(
                request.EnrollmentId, 
                request.CardDetails.CardholderName, 
                request.CardDetails.CardNumber, 
                request.CardDetails.ExpiryMonth.ToString(), 
                request.CardDetails.ExpiryYear.ToString(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentId);

        _paymentServiceMock
            .Setup(s => s.GetPaymentByIdAsync(paymentId))
            .ReturnsAsync(payment);

        // Act
        var result = await _controller.ProcessPayment(request);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetPayment", createdAtActionResult.ActionName);
        var returnValue = Assert.IsType<Payment>(createdAtActionResult.Value);
        Assert.Equal(paymentId, returnValue.Id);
        Assert.Equal(request.Amount, returnValue.Amount);
    }

    [Fact]
    public async Task GetPayment_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var cardDetails = new CardDetails(
            "John Doe",
            "4111111111111111",
            "12",
            "2030"
        );
        
        var payment = new Payment(studentId, enrollmentId, 99.99m, cardDetails);

        // Set payment ID through reflection
        typeof(Payment).GetProperty("Id").SetValue(payment, paymentId);
        
        // Mark payment as successful to set transaction ID
        payment.MarkAsSuccess("TXN-123456");

        _paymentServiceMock
            .Setup(s => s.GetPaymentByIdAsync(paymentId))
            .ReturnsAsync(payment);

        // Act
        var result = await _controller.GetPayment(paymentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<Payment>(okResult.Value);
        Assert.Equal(paymentId, returnValue.Id);
        Assert.Equal(99.99m, returnValue.Amount);
        Assert.Equal(StatusPagamento.Aprovado, returnValue.Status);
        Assert.Equal("TXN-123456", returnValue.TransactionId);
    }

    [Fact]
    public async Task GetPayment_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var paymentId = Guid.NewGuid();

        _paymentServiceMock
            .Setup(s => s.GetPaymentByIdAsync(paymentId))
            .ThrowsAsync(new NotFoundException("Payment", paymentId));

        // Act
        var result = await _controller.GetPayment(paymentId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task RefundPayment_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var request = new RefundProcessRequest
        {
            Reason = "Customer request"
        };

        var studentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var cardDetails = new CardDetails(
            "John Doe",
            "4111111111111111",
            "12",
            "2030"
        );
        
        var payment = new Payment(studentId, enrollmentId, 99.99m, cardDetails);

        // Set payment ID through reflection
        typeof(Payment).GetProperty("Id").SetValue(payment, paymentId);
        
        // Mark payment as successful then refunded
        payment.MarkAsSuccess("TXN-123456");
        payment.MarkAsRefunded(request.Reason);

        _paymentServiceMock
            .Setup(s => s.RefundPaymentAsync(paymentId, request.Reason, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _paymentServiceMock
            .Setup(s => s.GetPaymentByIdAsync(paymentId))
            .ReturnsAsync(payment);

        // Act
        var result = await _controller.RefundPayment(paymentId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<Payment>(okResult.Value);
        Assert.Equal(paymentId, returnValue.Id);
        Assert.Equal(StatusPagamento.Reembolsado, returnValue.Status);
        Assert.Equal(request.Reason, returnValue.RefundReason);
    }

    [Fact]
    public async Task RefundPayment_WithInvalidPaymentId_ReturnsNotFound()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var request = new RefundProcessRequest
        {
            Reason = "Customer request"
        };

        _paymentServiceMock
            .Setup(s => s.RefundPaymentAsync(paymentId, request.Reason, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Payment", paymentId));

        // Act
        var result = await _controller.RefundPayment(paymentId, request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task RefundPayment_AlreadyRefunded_ReturnsUnprocessableEntity()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var request = new RefundProcessRequest
        {
            Reason = "Customer request"
        };

        _paymentServiceMock
            .Setup(s => s.RefundPaymentAsync(paymentId, request.Reason, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Payment has already been refunded"));

        // Act
        var result = await _controller.RefundPayment(paymentId, request);

        // Assert
        Assert.IsType<UnprocessableEntityObjectResult>(result);
    }
} 