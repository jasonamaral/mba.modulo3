using FluentValidation;

namespace FluencyHub.StudentManagement.Application.Commands.EnrollStudent;

public class EnrollStudentCommandValidator : AbstractValidator<EnrollStudentCommand>
{
    public EnrollStudentCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty()
            .WithMessage("Student ID is required");

        RuleFor(x => x.CourseId)
            .NotEmpty()
            .WithMessage("Course ID is required");

        RuleFor(x => x.EnrollmentDate)
            .NotEmpty()
            .WithMessage("Enrollment date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Enrollment date cannot be more than 1 day in the future");

        RuleFor(x => x.DiscountPercentage)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Discount percentage cannot be negative")
            .LessThanOrEqualTo(100)
            .WithMessage("Discount percentage cannot exceed 100%")
            .When(x => x.DiscountPercentage.HasValue);
    }
} 