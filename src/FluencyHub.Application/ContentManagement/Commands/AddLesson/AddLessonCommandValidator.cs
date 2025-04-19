using FluentValidation;

namespace FluencyHub.Application.ContentManagement.Commands.AddLesson;

public class AddLessonCommandValidator : AbstractValidator<AddLessonCommand>
{
    public AddLessonCommandValidator()
    {
        RuleFor(v => v.CourseId)
            .NotEmpty().WithMessage("Course ID is required.");

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Lesson title is required.")
            .MaximumLength(200).WithMessage("Lesson title must not exceed 200 characters.");

        RuleFor(v => v.Content)
            .NotEmpty().WithMessage("Lesson content is required.")
            .MaximumLength(10000).WithMessage("Lesson content must not exceed 10000 characters.");

        RuleFor(v => v.MaterialUrl)
            .MaximumLength(500).WithMessage("Material URL must not exceed 500 characters.")
            .Must(BeValidUrl).When(m => !string.IsNullOrEmpty(m.MaterialUrl))
            .WithMessage("Material URL must be a valid URL.");
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}