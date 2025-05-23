using MediatR;

namespace FluencyHub.SharedKernel.Events.Integration;

public interface IIntegrationEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : INotification
{
} 