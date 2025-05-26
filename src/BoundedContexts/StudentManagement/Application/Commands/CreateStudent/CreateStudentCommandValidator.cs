using FluentValidation;

namespace FluencyHub.StudentManagement.Application.Commands.CreateStudent;

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MinimumLength(2).WithMessage("First name must be at least 2 characters")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");
            
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");
            
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters");
            
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");
            
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number must be a valid international format");
            
        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .Must(BeValidAge).WithMessage("Student must be at least 13 years old and not older than 120 years");
            
        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Address));
            
        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.City));
            
        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage("State must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.State));
            
        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Country));
            
        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.PostalCode));
    }
    
    private bool BeValidAge(DateTime dateOfBirth)
    {
        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
        
        return age >= 13 && age <= 120;
    }
} 