namespace FluencyHub.StudentManagement.Application.Common.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException() 
        : base("Uma requisição inválida ocorreu.")
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