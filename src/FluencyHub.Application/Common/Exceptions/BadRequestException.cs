namespace FluencyHub.Application.Common.Exceptions;

/// <summary>
/// Exception lançada quando a requisição é inválida.
/// </summary>
public class BadRequestException : Exception
{
    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="BadRequestException"/>.
    /// </summary>
    public BadRequestException()
        : base()
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="BadRequestException"/> com uma mensagem de erro especificada.
    /// </summary>
    /// <param name="message">A mensagem que descreve o erro.</param>
    public BadRequestException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="BadRequestException"/> com uma mensagem de erro especificada
    /// e uma referência à exceção interna que é a causa deste erro.
    /// </summary>
    /// <param name="message">A mensagem que descreve o erro.</param>
    /// <param name="innerException">A exceção que é a causa da exceção atual.</param>
    public BadRequestException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
} 