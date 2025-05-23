namespace FluencyHub.PaymentProcessing.Application.Common.Exceptions;

public class PaymentNotFoundException : Exception
{
    public PaymentNotFoundException(Guid id)
        : base($"Pagamento com ID {id} não foi encontrado.")
    {
    }
} 