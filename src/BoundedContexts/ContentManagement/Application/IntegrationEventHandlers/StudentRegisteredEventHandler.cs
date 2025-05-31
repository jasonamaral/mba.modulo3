using FluencyHub.SharedKernel.Events.Integration;

namespace FluencyHub.ContentManagement.Application.IntegrationEventHandlers;

public class StudentRegisteredEventHandler : IIntegrationEventHandler<StudentRegisteredEvent>
{
    public StudentRegisteredEventHandler()
    {
    }

    public Task Handle(StudentRegisteredEvent notification, CancellationToken cancellationToken)
    {
        // Aqui você pode implementar a lógica específica do BC ContentManagement
        // Por exemplo: criar um perfil de estudante no sistema de conteúdo,
        // configurar permissões iniciais, etc.

        return Task.CompletedTask;
    }
} 