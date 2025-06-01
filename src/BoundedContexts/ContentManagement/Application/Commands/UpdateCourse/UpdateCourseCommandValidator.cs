using FluentValidation;

namespace FluencyHub.ContentManagement.Application.Commands.UpdateCourse;

public class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O ID do curso é obrigatório");
            
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome do curso é obrigatório")
            .MaximumLength(100).WithMessage("O nome do curso não deve exceder 100 caracteres");
            
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A descrição do curso é obrigatória")
            .MaximumLength(2000).WithMessage("A descrição do curso não deve exceder 2000 caracteres");
            
        RuleFor(x => x.Syllabus)
            .NotEmpty().WithMessage("O programa é obrigatório")
            .MaximumLength(5000).WithMessage("O programa não deve exceder 5000 caracteres");
            
        RuleFor(x => x.LearningObjectives)
            .NotEmpty().WithMessage("Os objetivos de aprendizagem são obrigatórios")
            .MaximumLength(2000).WithMessage("Os objetivos de aprendizagem não devem exceder 2000 caracteres");
            
        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("O idioma é obrigatório")
            .MaximumLength(50).WithMessage("O idioma não deve exceder 50 caracteres");
            
        RuleFor(x => x.Level)
            .NotEmpty().WithMessage("O nível é obrigatório")
            .MaximumLength(50).WithMessage("O nível não deve exceder 50 caracteres");
            
        RuleFor(x => x.PreRequisites)
            .MaximumLength(1000).WithMessage("Os pré-requisitos não devem exceder 1000 caracteres");
            
        RuleFor(x => x.TargetAudience)
            .MaximumLength(1000).WithMessage("O público-alvo não deve exceder 1000 caracteres");
            
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("O preço deve ser maior ou igual a zero");
    }
} 