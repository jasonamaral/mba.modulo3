namespace FluencyHub.PaymentProcessing.Application.Common.Exceptions;

public class InvalidPaymentStatusException : Exception
{
    public InvalidPaymentStatusException(string message) : base(message)
    {
    }
} 