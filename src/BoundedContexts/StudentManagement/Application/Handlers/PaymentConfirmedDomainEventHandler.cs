using FluencyHub.PaymentProcessing.Domain.Events;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;
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
        _logger.LogInformation("Processando confirmação de pagamento para matrícula {EnrollmentId}", notification.EnrollmentId);
        
        var enrollment = await _enrollmentRepository.GetByIdAsync(notification.EnrollmentId);
        if (enrollment == null)
        {
            _logger.LogWarning("Matrícula não encontrada para ID: {EnrollmentId}", notification.EnrollmentId);
            return;
        }
        
        try
        {
            // Verificar se a matrícula já está ativa para evitar erro de duplicação
            if (enrollment.Status == StatusMatricula.Ativa)
            {
                _logger.LogInformation("Matrícula {EnrollmentId} já está ativa, ignorando ativação duplicada", notification.EnrollmentId);
                return;
            }
            
            // Ativar a matrícula apenas se estiver aguardando pagamento
            if (enrollment.Status == StatusMatricula.AguardandoPagamento)
            {
                enrollment.ActivateEnrollment();
                await _enrollmentRepository.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Matrícula {EnrollmentId} ativada com sucesso após confirmação do pagamento", notification.EnrollmentId);
            }
            else
            {
                _logger.LogWarning("Matrícula {EnrollmentId} não está em estado válido para ativação. Status atual: {Status}", 
                    notification.EnrollmentId, enrollment.Status);
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