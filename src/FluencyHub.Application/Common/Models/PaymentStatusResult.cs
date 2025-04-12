namespace FluencyHub.Application.Common.Models;

public class PaymentStatusResult
{
    public string TransactionId { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? StatusMessage { get; private set; }
    
    private PaymentStatusResult() 
    { 
        TransactionId = string.Empty;
    }
    
    public static PaymentStatusResult Create(string transactionId, PaymentStatus status, string? statusMessage = null)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID cannot be empty", nameof(transactionId));
            
        return new PaymentStatusResult
        {
            TransactionId = transactionId,
            Status = status,
            StatusMessage = statusMessage
        };
    }
}

public enum PaymentStatus
{
    Pending,
    Authorized,
    Completed,
    Failed,
    Refunded,
    Cancelled,
    Unknown
} 