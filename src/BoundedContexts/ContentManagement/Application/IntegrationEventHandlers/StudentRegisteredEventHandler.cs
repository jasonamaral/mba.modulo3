using FluencyHub.SharedKernel.Events.Integration;
using Microsoft.Extensions.Logging;

namespace FluencyHub.ContentManagement.Application.IntegrationEventHandlers;

public class StudentRegisteredEventHandler : IIntegrationEventHandler<StudentRegisteredEvent>
{
    private readonly ILogger<StudentRegisteredEventHandler> _logger;

    public StudentRegisteredEventHandler(ILogger<StudentRegisteredEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(StudentRegisteredEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "ContentManagement BC - Handling StudentRegisteredEvent for student {StudentId} - {StudentName}",
            notification.StudentId,
            notification.Name);

        // Aqui você pode implementar a lógica específica do BC ContentManagement
        // Por exemplo: criar um perfil de estudante no sistema de conteúdo,
        // configurar permissões iniciais, etc.

        return Task.CompletedTask;
    }
} 