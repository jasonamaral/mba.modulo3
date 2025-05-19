using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.PaymentProcessing.Domain;
using MediatR;

namespace FluencyHub.Application.PaymentProcessing.Commands.RefundPayment;

public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand>
{
    private readonly FluencyHub.Application.Common.Interfaces.IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;
    
    public RefundPaymentCommandHandler(
        FluencyHub.Application.Common.Interfaces.IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway)
    {
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
    }
    
    public async Task Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId);
        
        if (payment == null)
        {
            throw new NotFoundException(nameof(Payment), request.PaymentId);
        }
        
        if (!payment.IsSuccessful)
        {
            throw new InvalidOperationException("Only successful payments can be refunded");
        }
        
        if (payment.IsRefunded)
        {
            throw new InvalidOperationException("Payment has already been refunded");
        }
        
        if (string.IsNullOrEmpty(payment.TransactionId))
        {
            throw new InvalidOperationException("Payment transaction ID is missing");
        }
        
        var refundResult = await _paymentGateway.ProcessRefundAsync(
            payment.TransactionId,
            payment.Amount,
            request.Reason);
            
        if (refundResult.IsSuccessful)
        {
            payment.MarkAsRefunded(request.Reason);
            await _paymentRepository.SaveChangesAsync(cancellationToken);
        }
        else
        {
            throw new InvalidOperationException($"Refund failed: {refundResult.ErrorMessage}");
        }
    }
} 