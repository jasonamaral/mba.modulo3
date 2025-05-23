using FluencyHub.SharedKernel.Events.PaymentProcessing;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.StudentManagement.Application.Handlers;

public class EnrollmentActivatedEventHandler : INotificationHandler<EnrollmentActivatedEvent>
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICertificateRepository _certificateRepository;
    private readonly ILogger<EnrollmentActivatedEventHandler> _logger;
    
    public EnrollmentActivatedEventHandler(
        IEnrollmentRepository enrollmentRepository,
        ICertificateRepository certificateRepository,
        ILogger<EnrollmentActivatedEventHandler> logger)
    {
        _enrollmentRepository = enrollmentRepository;
        _certificateRepository = certificateRepository;
        _logger = logger;
    }
    
    public async Task Handle(EnrollmentActivatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling EnrollmentActivatedEvent for EnrollmentId: {EnrollmentId}", notification.EnrollmentId);
        
        var enrollment = await _enrollmentRepository.GetByIdAsync(notification.EnrollmentId);
        if (enrollment == null)
        {
            _logger.LogWarning("Enrollment not found for id: {EnrollmentId}", notification.EnrollmentId);
            return;
        }
        
        enrollment.ActivateEnrollment();
        _logger.LogInformation("Enrollment activated: {EnrollmentId}", enrollment.Id);
        
        await _enrollmentRepository.SaveChangesAsync(cancellationToken);
    }
} 