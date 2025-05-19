using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.PaymentProcessing.Domain;
using FluencyHub.StudentManagement.Domain;
using MediatR;

namespace FluencyHub.Application.PaymentProcessing.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Guid>
{
    private readonly FluencyHub.Application.Common.Interfaces.IEnrollmentRepository _enrollmentRepository;
    private readonly FluencyHub.Application.Common.Interfaces.IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;
    
    public ProcessPaymentCommandHandler(
        FluencyHub.Application.Common.Interfaces.IEnrollmentRepository enrollmentRepository,
        FluencyHub.Application.Common.Interfaces.IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway)
    {
        _enrollmentRepository = enrollmentRepository;
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
    }
    
    public async Task<Guid> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(request.EnrollmentId);
        
        if (enrollment == null)
        {
            throw new NotFoundException(nameof(Enrollment), request.EnrollmentId);
        }
        
        if (!enrollment.IsPendingPayment)
        {
            throw new InvalidOperationException("Payment can only be processed for enrollments with pending payment status");
        }
        
        var cardDetails = new CardDetails(
            request.CardHolderName,
            request.CardNumber,
            request.ExpiryMonth,
            request.ExpiryYear);
            
        var payment = new Payment(
            enrollment.StudentId,
            enrollment.Id,
            enrollment.Price,
            cardDetails);
            
        await _paymentRepository.AddAsync(payment);
        
        var paymentResult = await _paymentGateway.ProcessPaymentAsync(
            payment.Id.ToString(),
            payment.Amount,
            cardDetails);
            
        if (paymentResult.IsSuccessful)
        {
            payment.MarkAsSuccess(paymentResult.TransactionId);
            enrollment.ActivateEnrollment();
        }
        else
        {
            payment.MarkAsFailed(paymentResult.ErrorMessage);
        }
        
        await _paymentRepository.SaveChangesAsync(cancellationToken);
        await _enrollmentRepository.SaveChangesAsync(cancellationToken);
        
        return payment.Id;
    }
} 