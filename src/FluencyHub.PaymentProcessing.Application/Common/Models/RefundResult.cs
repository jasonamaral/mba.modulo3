namespace FluencyHub.PaymentProcessing.Application.Common.Models;

public class RefundResult
{
    public bool IsSuccessful { get; init; }
    public string OriginalTransactionId { get; init; }
    public string? RefundTransactionId { get; init; }
    public decimal RefundAmount { get; init; }
    public string? ErrorMessage { get; init; }
    
    private RefundResult(bool isSuccessful, string originalTransactionId, string? refundTransactionId, decimal refundAmount, string? errorMessage)
    {
        IsSuccessful = isSuccessful;
        OriginalTransactionId = originalTransactionId ?? throw new ArgumentNullException(nameof(originalTransactionId));
        RefundTransactionId = refundTransactionId;
        RefundAmount = refundAmount;
        ErrorMessage = errorMessage;
    }
    
    public static RefundResult Success(string originalTransactionId, string refundTransactionId, decimal refundAmount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(originalTransactionId, nameof(originalTransactionId));
        ArgumentException.ThrowIfNullOrWhiteSpace(refundTransactionId, nameof(refundTransactionId));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(refundAmount, 0, nameof(refundAmount));
            
        return new RefundResult(true, originalTransactionId, refundTransactionId, refundAmount, null);
    }
    
    public static RefundResult Failure(string originalTransactionId, string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(originalTransactionId, nameof(originalTransactionId));
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage, nameof(errorMessage));
            
        return new RefundResult(false, originalTransactionId, null, 0, errorMessage);
    }
}
