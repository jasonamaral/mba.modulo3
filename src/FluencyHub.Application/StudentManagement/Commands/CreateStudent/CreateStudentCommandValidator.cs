using FluentValidation;

namespace FluencyHub.Application.StudentManagement.Commands.CreateStudent;

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .Must(BeAValidDate).WithMessage("Date of birth must be a valid date")
            .Must(BeAtLeast16YearsOld).WithMessage("Student must be at least 16 years old");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\d{10,11}$").WithMessage("Phone number must contain between 10 and 11 digits");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
    }

    private bool BeAValidDate(DateTime date)
    {
        return !date.Equals(default(DateTime));
    }

    private bool BeAtLeast16YearsOld(DateTime dateOfBirth)
    {
        return dateOfBirth.AddYears(16) <= DateTime.Today;
    }
}