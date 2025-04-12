namespace FluencyHub.Application.Common.Models;

public class RefundResult
{
    public bool Succeeded { get; private set; }
    public string OriginalTransactionId { get; private set; }
    public string? RefundTransactionId { get; private set; }
    public decimal RefundAmount { get; private set; }
    public string? ErrorMessage { get; private set; }
    
    private RefundResult() 
    { 
        OriginalTransactionId = string.Empty;
    }
    
    public static RefundResult Success(string originalTransactionId, string refundTransactionId, decimal refundAmount)
    {
        if (string.IsNullOrWhiteSpace(originalTransactionId))
            throw new ArgumentException("Original transaction ID cannot be empty", nameof(originalTransactionId));
            
        if (string.IsNullOrWhiteSpace(refundTransactionId))
            throw new ArgumentException("Refund transaction ID cannot be empty", nameof(refundTransactionId));
            
        if (refundAmount <= 0)
            throw new ArgumentException("Refund amount must be positive", nameof(refundAmount));
        
        return new RefundResult
        {
            Succeeded = true,
            OriginalTransactionId = originalTransactionId,
            RefundTransactionId = refundTransactionId,
            RefundAmount = refundAmount,
            ErrorMessage = null
        };
    }
    
    public static RefundResult Failure(string originalTransactionId, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(originalTransactionId))
            throw new ArgumentException("Original transaction ID cannot be empty", nameof(originalTransactionId));
            
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty", nameof(errorMessage));
            
        return new RefundResult
        {
            Succeeded = false,
            OriginalTransactionId = originalTransactionId,
            RefundTransactionId = null,
            RefundAmount = 0,
            ErrorMessage = errorMessage
        };
    }
} 