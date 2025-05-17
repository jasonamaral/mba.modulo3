using FluencyHub.Domain.PaymentProcessing;
using FluencyHub.PaymentProcessing.Application.Common.Models;

namespace FluencyHub.PaymentProcessing.Application.Common.Interfaces;

public interface IPaymentService
{
    /// <summary>
    /// Process a payment using the payment gateway
    /// </summary>
    /// <param name="studentId">ID of the student making the payment</param>
    /// <param name="enrollmentId">ID of the enrollment being paid for</param>
    /// <param name="amount">Amount to be charged</param>
    /// <param name="cardDetails">Credit card details</param>
    /// <returns>Result of the payment process</returns>
    Task<FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult> ProcessPaymentAsync(
        Guid studentId, 
        Guid enrollmentId, 
        decimal amount, 
        CardDetails cardDetails);
    
    /// <summary>
    /// Get payment status from the payment gateway
    /// </summary>
    /// <param name="transactionId">Payment gateway transaction ID</param>
    /// <returns>Current status of the payment</returns>
    Task<FluencyHub.PaymentProcessing.Application.Common.Models.PaymentStatusResult> GetPaymentStatusAsync(string transactionId);
    
    /// <summary>
    /// Request a refund for a payment
    /// </summary>
    /// <param name="transactionId">Original transaction ID</param>
    /// <param name="amount">Amount to refund (can be partial)</param>
    /// <returns>Result of the refund request</returns>
    Task<FluencyHub.PaymentProcessing.Application.Common.Models.RefundResult> RequestRefundAsync(string transactionId, decimal amount);
} 