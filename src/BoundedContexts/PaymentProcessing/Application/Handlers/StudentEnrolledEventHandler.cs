using System;
using System.Threading;
using System.Threading.Tasks;
using FluencyHub.PaymentProcessing.Application.Common.Interfaces;
using FluencyHub.PaymentProcessing.Domain;
using FluencyHub.SharedKernel.Events.StudentManagement;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.PaymentProcessing.Application.Handlers;

public class StudentEnrolledEventHandler : INotificationHandler<StudentEnrolledEvent>
{
    private readonly IPaymentApplicationService _paymentService;
    private readonly ILogger<StudentEnrolledEventHandler> _logger;

    public StudentEnrolledEventHandler(
        IPaymentApplicationService paymentService,
        ILogger<StudentEnrolledEventHandler> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task Handle(StudentEnrolledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling StudentEnrolledEvent for StudentId: {StudentId}, CourseId: {CourseId}, EnrollmentId: {EnrollmentId}", 
            notification.StudentId, notification.CourseId, notification.EnrollmentId);
        
        try
        {
            await _paymentService.ProcessPaymentAsync(
                notification.StudentId,
                "4111111111111111",
                "Pagamento Automático",
                "12/2025",
                "123",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar pagamento automático para matrícula {EnrollmentId}", notification.EnrollmentId);
            throw;
        }
    }
} 