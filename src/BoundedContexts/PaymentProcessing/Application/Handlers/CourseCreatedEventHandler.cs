using FluencyHub.SharedKernel.Events.ContentManagement;
using MediatR;

namespace FluencyHub.PaymentProcessing.Application.Handlers;

public class CourseCreatedEventHandler : INotificationHandler<CourseCreatedEvent>
{
    public CourseCreatedEventHandler()
    {
    }
    
    public Task Handle(CourseCreatedEvent notification, CancellationToken cancellationToken)
    {
        // No momento, não precisamos fazer nada específico no contexto de pagamento quando um curso é criado,
        // mas no futuro podemos adicionar lógica como criar um registro de produto no sistema de pagamento, etc.
        
        return Task.CompletedTask;
    }
} 