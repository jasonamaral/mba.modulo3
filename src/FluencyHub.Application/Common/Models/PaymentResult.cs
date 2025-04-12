namespace FluencyHub.Application.Common.Models;

public class PaymentResult
{
    public bool Succeeded { get; private set; }
    public string? TransactionId { get; private set; }
    public string? ErrorMessage { get; private set; }
    
    private PaymentResult() { }
    
    public static PaymentResult Success(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID cannot be empty", nameof(transactionId));
            
        return new PaymentResult
        {
            Succeeded = true,
            TransactionId = transactionId,
            ErrorMessage = null
        };
    }
    
    public static PaymentResult Failure(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty", nameof(errorMessage));
            
        return new PaymentResult
        {
            Succeeded = false,
            TransactionId = null,
            ErrorMessage = errorMessage
        };
    }
} 