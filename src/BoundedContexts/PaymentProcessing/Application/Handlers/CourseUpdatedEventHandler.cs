using FluencyHub.SharedKernel.Events.ContentManagement;
using FluencyHub.PaymentProcessing.Application.Common.Interfaces;
using MediatR;

namespace FluencyHub.PaymentProcessing.Application.Handlers;

public class CourseUpdatedEventHandler : INotificationHandler<CourseUpdatedEvent>
{
    private readonly IPaymentRepository _paymentRepository;
    
    public CourseUpdatedEventHandler(
        IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }
    
    public Task Handle(CourseUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // Aqui poderíamos implementar uma lógica para atualizar valores de pagamentos pendentes
        // ou enviar notificações sobre mudança de preço para alunos interessados
        
        return Task.CompletedTask;
    }
} 