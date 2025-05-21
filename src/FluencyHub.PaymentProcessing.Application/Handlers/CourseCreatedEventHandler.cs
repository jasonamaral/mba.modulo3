using FluencyHub.SharedKernel.Events.ContentManagement;
using FluencyHub.PaymentProcessing.Application.Common.Interfaces;
using FluencyHub.PaymentProcessing.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.PaymentProcessing.Application.Handlers;

public class CourseCreatedEventHandler : INotificationHandler<CourseCreatedEvent>
{
    private readonly ILogger<CourseCreatedEventHandler> _logger;
    
    public CourseCreatedEventHandler(ILogger<CourseCreatedEventHandler> logger)
    {
        _logger = logger;
    }
    
    public Task Handle(CourseCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling CourseCreatedEvent for CourseId: {CourseId}, Name: {Title}, Price: {Price}", 
            notification.CourseId, notification.Title, notification.Price);
        
        // No momento, não precisamos fazer nada específico no contexto de pagamento quando um curso é criado,
        // mas no futuro podemos adicionar lógica como criar um registro de produto no sistema de pagamento, etc.
        
        return Task.CompletedTask;
    }
} 