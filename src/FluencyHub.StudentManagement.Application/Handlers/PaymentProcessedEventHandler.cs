using FluencyHub.SharedKernel.Events.PaymentProcessing;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.StudentManagement.Application.Handlers;

public class PaymentProcessedEventHandler : INotificationHandler<PaymentProcessedEvent>
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ILogger<PaymentProcessedEventHandler> _logger;
    
    public PaymentProcessedEventHandler(
        IEnrollmentRepository enrollmentRepository,
        ILogger<PaymentProcessedEventHandler> logger)
    {
        _enrollmentRepository = enrollmentRepository;
        _logger = logger;
    }
    
    public async Task Handle(PaymentProcessedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling PaymentProcessedEvent for EnrollmentId: {EnrollmentId}", notification.EnrollmentId);
        
        var enrollment = await _enrollmentRepository.GetByIdAsync(notification.EnrollmentId);
        if (enrollment == null)
        {
            _logger.LogWarning("Enrollment not found for id: {EnrollmentId}", notification.EnrollmentId);
            return;
        }
        
        if (notification.IsSuccessful)
        {
            enrollment.ProcessPaymentSuccess(notification.PaymentId, notification.Amount);
            _logger.LogInformation("Payment processed successfully for enrollment: {EnrollmentId}", enrollment.Id);
        }
        else
        {
            _logger.LogWarning("Payment failed for enrollment: {EnrollmentId}, reason: {Reason}", 
                enrollment.Id, notification.ErrorMessage);
            enrollment.ProcessPaymentFailure(notification.ErrorMessage);
        }
        
        await _enrollmentRepository.SaveChangesAsync(cancellationToken);
    }
} 