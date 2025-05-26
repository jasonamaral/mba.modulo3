using FluentValidation.Results;

namespace FluencyHub.StudentManagement.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationException()
        : base("Uma ou mais falhas de validação ocorreram.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
} 