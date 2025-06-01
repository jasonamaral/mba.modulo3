using FluentValidation;

namespace FluencyHub.StudentManagement.Application.Commands.EnrollStudent;

public class EnrollStudentCommandValidator : AbstractValidator<EnrollStudentCommand>
{
    public EnrollStudentCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("O ID do estudante é obrigatório");

        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("O ID do curso é obrigatório");

        RuleFor(x => x.EnrollmentDate)
            .NotEmpty().WithMessage("A data de matrícula é obrigatória")
            .Must(BeValidEnrollmentDate).WithMessage("A data de matrícula não pode ser mais de 1 dia no futuro");

        RuleFor(x => x.DiscountPercentage)
            .GreaterThanOrEqualTo(0).WithMessage("A porcentagem de desconto não pode ser negativa")
            .LessThanOrEqualTo(100).WithMessage("A porcentagem de desconto não pode exceder 100%");
    }

    private static bool BeValidEnrollmentDate(DateTime enrollmentDate)
    {
        return enrollmentDate <= DateTime.UtcNow.AddDays(1);
    }
} 