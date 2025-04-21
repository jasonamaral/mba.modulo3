using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.Common.Models;
using FluencyHub.Domain.PaymentProcessing.Events;
using FluencyHub.Domain.StudentManagement;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.Application.PaymentProcessing.EventHandlers;

public class PaymentRejectedDomainEventHandler : INotificationHandler<DomainEventNotification<PaymentRejectedDomainEvent>>
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ILogger<PaymentRejectedDomainEventHandler> _logger;

    public PaymentRejectedDomainEventHandler(
        IEnrollmentRepository enrollmentRepository,
        ILogger<PaymentRejectedDomainEventHandler> logger)
    {
        _enrollmentRepository = enrollmentRepository;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<PaymentRejectedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogInformation("Processing payment rejection for enrollmentId: {enrollmentId}", domainEvent.EnrollmentId);

        var enrollment = await _enrollmentRepository.GetByIdAsync(domainEvent.EnrollmentId);
        
        if (enrollment == null)
        {
            _logger.LogWarning("Enrollment not found for ID: {enrollmentId}", domainEvent.EnrollmentId);
            throw new NotFoundException(nameof(Enrollment), domainEvent.EnrollmentId);
        }
        
        _logger.LogInformation("Payment failed for enrollment {enrollmentId}. Reason: {reason}", 
            domainEvent.EnrollmentId, domainEvent.FailureReason);
            
        // Enrollment permanece no estado de "PendingPayment", conforme solicitado
    }
} 