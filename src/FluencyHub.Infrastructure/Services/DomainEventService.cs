using FluencyHub.Application.Common.Models;
using FluencyHub.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FluencyHub.Infrastructure.Services;

public class DomainEventService : Application.Common.Interfaces.IDomainEventService
{
    private readonly ILogger<DomainEventService> _logger;
    private readonly IPublisher _mediator;

    public DomainEventService(ILogger<DomainEventService> logger, IPublisher mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task PublishAsync(DomainEvent domainEvent)
    {
        _logger.LogInformation("Publishing domain event: {event}", domainEvent.GetType().Name);
        
        // Criamos um tipo genérico baseado no tipo concreto do evento de domínio
        Type notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        
        // Criamos uma instância do adapter
        var notification = Activator.CreateInstance(notificationType, domainEvent);
        
        // Publicamos o adapter
        await _mediator.Publish(notification);
    }
} 