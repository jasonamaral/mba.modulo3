using FluencyHub.SharedKernel.Events.Integration;
using Microsoft.Extensions.Logging;

namespace FluencyHub.PaymentProcessing.Application.IntegrationEventHandlers;

public class EnrollmentCreatedEventHandler : IIntegrationEventHandler<EnrollmentCreatedEvent>
{
    private readonly ILogger<EnrollmentCreatedEventHandler> _logger;

    public EnrollmentCreatedEventHandler(ILogger<EnrollmentCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(EnrollmentCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "PaymentProcessing BC - Handling EnrollmentCreatedEvent for enrollment {EnrollmentId} - Student: {StudentId}, Course: {CourseId}, Price: {Price}",
            notification.EnrollmentId,
            notification.StudentId,
            notification.CourseId,
            notification.Price);

        // Aqui você pode implementar a lógica específica do BC PaymentProcessing
        // Por exemplo: criar uma fatura, iniciar o processo de cobrança,
        // verificar métodos de pagamento disponíveis, etc.

        return Task.CompletedTask;
    }
} 