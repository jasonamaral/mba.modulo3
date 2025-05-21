using FluencyHub.SharedKernel.Events.ContentManagement;
using FluencyHub.PaymentProcessing.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.PaymentProcessing.Application.Handlers;

public class CourseUpdatedEventHandler : INotificationHandler<CourseUpdatedEvent>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<CourseUpdatedEventHandler> _logger;
    
    public CourseUpdatedEventHandler(
        IPaymentRepository paymentRepository,
        ILogger<CourseUpdatedEventHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }
    
    public Task Handle(CourseUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling CourseUpdatedEvent for CourseId: {CourseId}, Name: {Title}, Price: {Price}", 
            notification.CourseId, notification.Title, notification.Price);
        
        // Aqui poderíamos implementar uma lógica para atualizar valores de pagamentos pendentes
        // ou enviar notificações sobre mudança de preço para alunos interessados
        
        return Task.CompletedTask;
    }
} 