namespace FluencyHub.SharedKernel.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException()
        : base("O recurso solicitado não foi encontrado.")
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public NotFoundException(string name, object key)
        : base($"Entidade \"{name}\" ({key}) não foi encontrada.")
    {
    }
} 