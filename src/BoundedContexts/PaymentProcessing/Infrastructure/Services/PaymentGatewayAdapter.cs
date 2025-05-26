using FluencyHub.PaymentProcessing.Domain;
using FluencyHub.PaymentProcessing.Application.Common.Interfaces;
using FluencyHub.PaymentProcessing.Application.Common.Models;

namespace FluencyHub.PaymentProcessing.Infrastructure.Services;

public class PaymentGatewayAdapter : Domain.IPaymentGateway
{
    private readonly Application.Common.Interfaces.IPaymentGateway _applicationGateway;

    public PaymentGatewayAdapter(Application.Common.Interfaces.IPaymentGateway applicationGateway)
    {
        _applicationGateway = applicationGateway ?? throw new ArgumentNullException(nameof(applicationGateway));
    }

    public async Task<Domain.PaymentGatewayResult> ProcessPaymentAsync(string paymentId, decimal amount, Domain.CardDetails cardDetails)
    {
        var applicationCardDetails = new Application.Common.Models.CardDetails
        {
            CardHolderName = cardDetails.CardHolderName,
            CardNumber = cardDetails.MaskedCardNumber,
            MaskedCardNumber = cardDetails.MaskedCardNumber,
            ExpiryMonth = cardDetails.ExpiryMonth,
            ExpiryYear = cardDetails.ExpiryYear,
            Cvv = "123"
        };

        var result = await _applicationGateway.ProcessPaymentAsync(paymentId, amount, applicationCardDetails);
        
        return new Domain.PaymentGatewayResult
        {
            IsSuccessful = result.IsSuccessful,
            TransactionId = result.TransactionId,
            ErrorMessage = result.ErrorMessage
        };
    }

    public async Task<Domain.PaymentGatewayResult> ProcessRefundAsync(string transactionId, decimal amount, string reason)
    {
        var result = await _applicationGateway.ProcessRefundAsync(transactionId, amount, reason);
        
        return new Domain.PaymentGatewayResult
        {
            IsSuccessful = result.IsSuccessful,
            TransactionId = result.RefundTransactionId,
            ErrorMessage = result.ErrorMessage
        };
    }
} 