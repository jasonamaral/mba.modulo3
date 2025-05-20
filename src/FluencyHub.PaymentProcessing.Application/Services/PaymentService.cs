using FluencyHub.PaymentProcessing.Application.Common.Exceptions;
using FluencyHub.PaymentProcessing.Application.Common.Interfaces;
using FluencyHub.PaymentProcessing.Domain;
using FluencyHub.StudentManagement.Domain;
using Microsoft.Extensions.Logging;

namespace FluencyHub.PaymentProcessing.Application.Services;

public class PaymentService : IPaymentApplicationService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IEnrollmentRepository enrollmentRepository,
        FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway,
        ILogger<PaymentService> logger)
    {
        _enrollmentRepository = enrollmentRepository;
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    public async Task<Guid> ProcessPaymentAsync(
        Guid enrollmentId, 
        string cardHolderName, 
        string cardNumber, 
        string expiryMonth, 
        string expiryYear, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing payment for enrollment: {enrollmentId}", enrollmentId);
        
        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);
        
        if (enrollment == null)
        {
            _logger.LogWarning("Enrollment not found: {enrollmentId}", enrollmentId);
            throw new NotFoundException(nameof(Enrollment), enrollmentId);
        }
        
        if (!enrollment.IsPendingPayment)
        {
            _logger.LogWarning("Enrollment is not in pending payment status: {enrollmentId}, Status: {status}", 
                enrollmentId, enrollment.Status);
            throw new InvalidOperationException("Payment can only be processed for enrollments with pending payment status");
        }
        
        var cardDetails = new CardDetails(
            cardHolderName,
            cardNumber,
            expiryMonth,
            expiryYear);
            
        var payment = new Payment(
            enrollment.StudentId,
            enrollment.Id,
            enrollment.Price,
            cardDetails);
            
        await _paymentRepository.AddAsync(payment);
        
        var paymentResult = await _paymentGateway.ProcessPaymentAsync(
            payment.Id.ToString(),
            payment.Amount,
            cardDetails);
            
        if (paymentResult.IsSuccessful)
        {
            _logger.LogInformation("Payment successful for enrollment: {enrollmentId}, TransactionId: {transactionId}", 
                enrollmentId, paymentResult.TransactionId);
            payment.MarkAsSuccess(paymentResult.TransactionId);
        }
        else
        {
            _logger.LogWarning("Payment failed for enrollment: {enrollmentId}, Reason: {reason}", 
                enrollmentId, paymentResult.ErrorMessage);
            payment.MarkAsFailed(paymentResult.ErrorMessage);
        }
        
        await _paymentRepository.SaveChangesAsync(cancellationToken);
        
        return payment.Id;
    }

    public async Task<Payment> GetPaymentByIdAsync(Guid paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        
        if (payment == null)
        {
            throw new NotFoundException(nameof(Payment), paymentId);
        }
        
        return payment;
    }

    public async Task RefundPaymentAsync(
        Guid paymentId, 
        string reason, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing refund for payment: {paymentId}", paymentId);
        
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        
        if (payment == null)
        {
            _logger.LogWarning("Payment not found: {paymentId}", paymentId);
            throw new NotFoundException(nameof(Payment), paymentId);
        }
        
        if (!payment.IsSuccessful)
        {
            _logger.LogWarning("Only successful payments can be refunded: {paymentId}, Status: {status}", 
                paymentId, payment.Status);
            throw new InvalidOperationException("Only successful payments can be refunded");
        }
        
        if (payment.IsRefunded)
        {
            _logger.LogWarning("Payment has already been refunded: {paymentId}", paymentId);
            throw new InvalidOperationException("Payment has already been refunded");
        }
        
        if (string.IsNullOrEmpty(payment.TransactionId))
        {
            _logger.LogWarning("Payment transaction ID is missing: {paymentId}", paymentId);
            throw new InvalidOperationException("Payment transaction ID is missing");
        }
        
        var refundResult = await _paymentGateway.ProcessRefundAsync(
            payment.TransactionId,
            payment.Amount,
            reason);
            
        if (refundResult.IsSuccessful)
        {
            _logger.LogInformation("Refund successful for payment: {paymentId}", paymentId);
            payment.MarkAsRefunded(reason);
            await _paymentRepository.SaveChangesAsync(cancellationToken);
        }
        else
        {
            _logger.LogWarning("Refund failed for payment: {paymentId}, Reason: {reason}", 
                paymentId, refundResult.ErrorMessage);
            throw new InvalidOperationException($"Refund failed: {refundResult.ErrorMessage}");
        }
    }
} 