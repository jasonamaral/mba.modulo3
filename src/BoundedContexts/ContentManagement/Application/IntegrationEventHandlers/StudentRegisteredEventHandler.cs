using FluencyHub.SharedKernel.Events.Integration;

namespace FluencyHub.ContentManagement.Application.IntegrationEventHandlers;

public class StudentRegisteredEventHandler : IIntegrationEventHandler<StudentRegisteredEvent>
{
    public StudentRegisteredEventHandler()
    {
    }

    public Task Handle(StudentRegisteredEvent notification, CancellationToken cancellationToken) => Task.CompletedTask;

}