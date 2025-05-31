namespace FluencyHub.StudentManagement.Infrastructure.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entidade '{name}' ({key}) não foi encontrada.")
    {
    }
} 