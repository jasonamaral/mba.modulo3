using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.Common.Models;
using FluencyHub.Domain.PaymentProcessing.Events;
using FluencyHub.Domain.StudentManagement;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.Application.PaymentProcessing.EventHandlers;

public class PaymentConfirmedDomainEventHandler : INotificationHandler<DomainEventNotification<PaymentConfirmedDomainEvent>>
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ILogger<PaymentConfirmedDomainEventHandler> _logger;

    public PaymentConfirmedDomainEventHandler(
        IEnrollmentRepository enrollmentRepository,
        ILogger<PaymentConfirmedDomainEventHandler> logger)
    {
        _enrollmentRepository = enrollmentRepository;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<PaymentConfirmedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogInformation("Processing payment confirmation for enrollmentId: {enrollmentId}", domainEvent.EnrollmentId);

        var enrollment = await _enrollmentRepository.GetByIdAsync(domainEvent.EnrollmentId);
        
        if (enrollment == null)
        {
            _logger.LogWarning("Enrollment not found for ID: {enrollmentId}", domainEvent.EnrollmentId);
            throw new NotFoundException(nameof(Enrollment), domainEvent.EnrollmentId);
        }
        
        if (enrollment.IsPendingPayment)
        {
            enrollment.ActivateEnrollment();
            await _enrollmentRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Enrollment {enrollmentId} activated successfully", domainEvent.EnrollmentId);
        }
        else
        {
            _logger.LogWarning("Enrollment {enrollmentId} is not in pending payment status: {status}", 
                domainEvent.EnrollmentId, enrollment.Status);
        }
    }
} 