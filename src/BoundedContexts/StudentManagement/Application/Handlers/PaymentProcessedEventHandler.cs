using FluencyHub.SharedKernel.Events.PaymentProcessing;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using MediatR;

namespace FluencyHub.StudentManagement.Application.Handlers;

public class PaymentProcessedEventHandler : INotificationHandler<PaymentProcessedEvent>
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    
    public PaymentProcessedEventHandler(
        IEnrollmentRepository enrollmentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
    }
    
    public async Task Handle(PaymentProcessedEvent notification, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(notification.EnrollmentId);
        
        if (enrollment == null)
        {
            return;
        }

        if (notification.IsSuccessful)
        {
            enrollment.ActivateEnrollment();
        }
        else
        {
            enrollment.CancelEnrollment();
        }

        await _enrollmentRepository.SaveChangesAsync(cancellationToken);
    }
} 