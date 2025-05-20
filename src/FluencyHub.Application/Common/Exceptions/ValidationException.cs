using FluentValidation.Results;

namespace FluencyHub.Application.Common.Exceptions;

/// <summary>
/// Exception lançada quando ocorrem erros de validação.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="ValidationException"/>.
    /// </summary>
    public ValidationException()
        : base("Um ou mais erros de validação ocorreram.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="ValidationException"/> com resultados de validação.
    /// </summary>
    /// <param name="failures">As falhas de validação.</param>
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    /// <summary>
    /// Obtém os erros de validação.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }
} 