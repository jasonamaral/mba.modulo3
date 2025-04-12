using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.PaymentProcessing;
using MediatR;

namespace FluencyHub.Application.PaymentProcessing.Queries.GetPaymentById;

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto>
{
    private readonly FluencyHub.Application.Common.Interfaces.IPaymentRepository _paymentRepository;
    
    public GetPaymentByIdQueryHandler(FluencyHub.Application.Common.Interfaces.IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }
    
    public async Task<PaymentDto> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.Id);
        
        if (payment == null)
        {
            throw new NotFoundException(nameof(Payment), request.Id);
        }
        
        return new PaymentDto
        {
            Id = payment.Id,
            StudentId = payment.StudentId,
            EnrollmentId = payment.EnrollmentId,
            Amount = payment.Amount,
            Status = payment.Status.ToString(),
            TransactionId = payment.TransactionId,
            FailureReason = payment.FailureReason,
            RefundReason = payment.RefundReason,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt
        };
    }
} 