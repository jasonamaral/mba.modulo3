namespace FluencyHub.PaymentProcessing.Application.Common.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException() 
        : base("Ocorreu uma requisição inválida.")
    {
    }

    public BadRequestException(string message) 
        : base(message)
    {
    }

    public BadRequestException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
} 