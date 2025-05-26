using FluentValidation;

namespace FluencyHub.ContentManagement.Application.Commands.AddLesson;

public class AddLessonCommandValidator : AbstractValidator<AddLessonCommand>
{
    public AddLessonCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Course ID is required");
            
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Lesson title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");
            
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Lesson description is required")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");
            
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Lesson content is required")
            .MaximumLength(10000).WithMessage("Content must not exceed 10,000 characters");
            
        RuleFor(x => x.Order)
            .GreaterThan(0).WithMessage("Order must be greater than 0");
            
        RuleFor(x => x.DurationMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("Duration must be greater than or equal to 0");
            
        RuleFor(x => x.VideoUrl)
            .Must(BeValidUrl).WithMessage("Video URL must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.VideoUrl));
    }
    
    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;
            
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
} 