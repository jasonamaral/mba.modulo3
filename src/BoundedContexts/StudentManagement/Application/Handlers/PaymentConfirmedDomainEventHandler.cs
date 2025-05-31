using FluencyHub.PaymentProcessing.Domain.Events;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.StudentManagement.Application.Handlers;

public class PaymentConfirmedDomainEventHandler : INotificationHandler<PaymentConfirmedDomainEvent>
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
    
    public async Task Handle(PaymentConfirmedDomainEvent notification, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(notification.EnrollmentId);
        if (enrollment == null)
        {
            return;
        }
        
        try
        {
            // Verificar se a matrícula já está ativa para evitar erro de duplicação
            if (enrollment.Status == StatusMatricula.Ativa)
            {
                return;
            }
            
            // Ativar a matrícula apenas se estiver aguardando pagamento
            if (enrollment.Status == StatusMatricula.AguardandoPagamento)
            {
                enrollment.ActivateEnrollment();
                await _enrollmentRepository.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar matrícula {EnrollmentId} após confirmação do pagamento", 
                notification.EnrollmentId);
            throw;
        }
    }
} 