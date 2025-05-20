namespace FluencyHub.Application.Common.Exceptions;

/// <summary>
/// Exception lançada quando uma entidade não é encontrada.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="NotFoundException"/>.
    /// </summary>
    public NotFoundException()
        : base()
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="NotFoundException"/> com uma mensagem de erro especificada.
    /// </summary>
    /// <param name="message">A mensagem que descreve o erro.</param>
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="NotFoundException"/> com uma mensagem de erro especificada
    /// e uma referência à exceção interna que é a causa deste erro.
    /// </summary>
    /// <param name="message">A mensagem que descreve o erro.</param>
    /// <param name="innerException">A exceção que é a causa da exceção atual.</param>
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="NotFoundException"/> para entidade não encontrada.
    /// </summary>
    /// <param name="name">O nome da entidade.</param>
    /// <param name="key">A chave da entidade.</param>
    public NotFoundException(string name, object key)
        : base($"Entidade \"{name}\" ({key}) não foi encontrada.")
    {
    }
} 