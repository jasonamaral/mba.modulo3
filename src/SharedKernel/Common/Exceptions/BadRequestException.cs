namespace FluencyHub.SharedKernel.Common.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException()
        : base("A solicitação é inválida.")
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

    public BadRequestException(string name, object key)
        : base($"Solicitação inválida para \"{name}\" ({key}).")
    {
    }
} 