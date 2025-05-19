using FluencyHub.PaymentProcessing.Domain;

namespace FluencyHub.Application.Common.Interfaces;

public interface IPaymentApplicationService
{
    Task<Guid> ProcessPaymentAsync(
        Guid enrollmentId, 
        string cardHolderName, 
        string cardNumber, 
        string expiryMonth, 
        string expiryYear, 
        CancellationToken cancellationToken = default);
        
    Task<Payment> GetPaymentByIdAsync(Guid paymentId);
    
    Task RefundPaymentAsync(
        Guid paymentId, 
        string reason, 
        CancellationToken cancellationToken = default);
}