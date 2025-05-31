using FluencyHub.SharedKernel.Events.Integration;

namespace FluencyHub.PaymentProcessing.Application.IntegrationEventHandlers;

public class EnrollmentCreatedEventHandler : IIntegrationEventHandler<EnrollmentCreatedEvent>
{
    public EnrollmentCreatedEventHandler()
    {
    }

    public Task Handle(EnrollmentCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Aqui você pode implementar a lógica específica do BC PaymentProcessing
        // Por exemplo: criar uma fatura, iniciar o processo de cobrança,
        // verificar métodos de pagamento disponíveis, etc.

        return Task.CompletedTask;
    }
} 