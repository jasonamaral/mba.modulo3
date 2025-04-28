namespace FluencyHub.Domain.PaymentProcessing;

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(Payment payment);
    Task<PaymentResult> RefundPaymentAsync(Payment payment);
}

public class PaymentResult
{
    public bool IsSuccessful { get; set; }
    public string? TransactionId { get; set; }
    public string? Message { get; set; }
} 