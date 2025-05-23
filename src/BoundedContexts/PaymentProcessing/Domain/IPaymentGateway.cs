using System.Threading.Tasks;

namespace FluencyHub.PaymentProcessing.Domain
{
    public interface IPaymentGateway
    {
        Task<PaymentGatewayResult> ProcessPaymentAsync(string paymentId, decimal amount, CardDetails cardDetails);
        Task<PaymentGatewayResult> ProcessRefundAsync(string transactionId, decimal amount, string reason);
    }

    public class PaymentGatewayResult
    {
        public bool IsSuccessful { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }
    }
} 