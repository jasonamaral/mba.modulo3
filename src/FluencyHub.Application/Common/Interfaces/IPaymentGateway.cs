using FluencyHub.Domain.PaymentProcessing;

namespace FluencyHub.Application.Common.Interfaces;

public interface IPaymentGateway
{
    Task<PaymentResult> ProcessPaymentAsync(string orderId, decimal amount, CardDetails cardDetails);
    Task<RefundResult> ProcessRefundAsync(string transactionId, decimal amount, string reason);
}

public class PaymentResult
{
    public bool IsSuccessful { get; private set; }
    public string? TransactionId { get; private set; }
    public string? ErrorMessage { get; private set; }
    
    private PaymentResult(bool isSuccessful, string? transactionId, string? errorMessage)
    {
        IsSuccessful = isSuccessful;
        TransactionId = transactionId;
        ErrorMessage = errorMessage;
    }
    
    public static PaymentResult Success(string transactionId)
    {
        return new PaymentResult(true, transactionId, null);
    }
    
    public static PaymentResult Failure(string errorMessage)
    {
        return new PaymentResult(false, null, errorMessage);
    }
}

public class RefundResult
{
    public bool IsSuccessful { get; private set; }
    public string OriginalTransactionId { get; private set; }
    public string? RefundTransactionId { get; private set; }
    public decimal RefundAmount { get; private set; }
    public string? ErrorMessage { get; private set; }
    
    private RefundResult(bool isSuccessful, string originalTransactionId, string? refundTransactionId, decimal refundAmount, string? errorMessage)
    {
        IsSuccessful = isSuccessful;
        OriginalTransactionId = originalTransactionId;
        RefundTransactionId = refundTransactionId;
        RefundAmount = refundAmount;
        ErrorMessage = errorMessage;
    }
    
    public static RefundResult Success(string originalTransactionId, string refundTransactionId, decimal refundAmount)
    {
        return new RefundResult(true, originalTransactionId, refundTransactionId, refundAmount, null);
    }
    
    public static RefundResult Failure(string originalTransactionId, string errorMessage)
    {
        return new RefundResult(false, originalTransactionId, null, 0, errorMessage);
    }
} 