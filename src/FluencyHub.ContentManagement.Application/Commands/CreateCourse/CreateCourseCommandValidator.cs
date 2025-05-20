using FluentValidation;

namespace FluencyHub.ContentManagement.Application.Commands.CreateCourse;

public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Course name is required")
            .MaximumLength(100).WithMessage("Course name must not exceed 100 characters");
            
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Course description is required")
            .MaximumLength(2000).WithMessage("Course description must not exceed 2000 characters");
            
        RuleFor(x => x.Syllabus)
            .NotEmpty().WithMessage("Syllabus is required")
            .MaximumLength(5000).WithMessage("Syllabus must not exceed 5000 characters");
            
        RuleFor(x => x.LearningObjectives)
            .NotEmpty().WithMessage("Learning objectives are required")
            .MaximumLength(2000).WithMessage("Learning objectives must not exceed 2000 characters");
            
        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required")
            .MaximumLength(50).WithMessage("Language must not exceed 50 characters");
            
        RuleFor(x => x.Level)
            .NotEmpty().WithMessage("Level is required")
            .MaximumLength(50).WithMessage("Level must not exceed 50 characters");
            
        RuleFor(x => x.PreRequisites)
            .MaximumLength(1000).WithMessage("Prerequisites must not exceed 1000 characters");
            
        RuleFor(x => x.TargetAudience)
            .MaximumLength(1000).WithMessage("Target audience must not exceed 1000 characters");
    }
} 