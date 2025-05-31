using FluentValidation;

namespace FluencyHub.StudentManagement.Application.Commands.CreateStudent;

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("O nome é obrigatório")
            .MinimumLength(2).WithMessage("O nome deve ter pelo menos 2 caracteres")
            .MaximumLength(100).WithMessage("O nome não deve exceder 100 caracteres");
            
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("O sobrenome é obrigatório")
            .MinimumLength(2).WithMessage("O sobrenome deve ter pelo menos 2 caracteres")
            .MaximumLength(100).WithMessage("O sobrenome não deve exceder 100 caracteres");
            
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O email é obrigatório")
            .EmailAddress().WithMessage("O email deve ter um formato válido")
            .MaximumLength(256).WithMessage("O email não deve exceder 256 caracteres");
            
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória")
            .MinimumLength(6).WithMessage("A senha deve ter pelo menos 6 caracteres")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            .WithMessage("A senha deve conter pelo menos uma letra maiúscula, uma letra minúscula, um dígito e um caractere especial");
            
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("O número de telefone é obrigatório")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("O número de telefone deve estar em formato internacional válido");
            
        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("A data de nascimento é obrigatória")
            .Must(BeValidAge).WithMessage("O estudante deve ter pelo menos 13 anos e não mais de 120 anos");
            
        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("O endereço não deve exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Address));
            
        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("A cidade não deve exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.City));
            
        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage("O estado não deve exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.State));
            
        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("O país não deve exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Country));
            
        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("O código postal não deve exceder 20 caracteres")
            .When(x => !string.IsNullOrEmpty(x.PostalCode));
    }
    
    private static bool BeValidAge(DateTime dateOfBirth)
    {
        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
        
        return age >= 13 && age <= 120;
    }
} 