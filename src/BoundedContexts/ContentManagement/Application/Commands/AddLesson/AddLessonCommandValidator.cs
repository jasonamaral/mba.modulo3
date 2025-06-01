using FluentValidation;

namespace FluencyHub.ContentManagement.Application.Commands.AddLesson;

public class AddLessonCommandValidator : AbstractValidator<AddLessonCommand>
{
    public AddLessonCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("O ID do curso é obrigatório");
            
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("O título da lição é obrigatório")
            .MaximumLength(200).WithMessage("O título não deve exceder 200 caracteres");
            
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A descrição da lição é obrigatória")
            .MaximumLength(500).WithMessage("A descrição não deve exceder 500 caracteres");
            
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("O conteúdo da lição é obrigatório")
            .MaximumLength(10000).WithMessage("O conteúdo não deve exceder 10.000 caracteres");
            
        RuleFor(x => x.Order)
            .GreaterThan(0).WithMessage("A ordem deve ser maior que 0");
            
        RuleFor(x => x.DurationMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("A duração deve ser maior ou igual a 0");
            
        RuleFor(x => x.VideoUrl)
            .Must(BeValidUrl).WithMessage("A URL do vídeo deve ser uma URL válida")
            .When(x => !string.IsNullOrEmpty(x.VideoUrl));
    }
    
    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;
            
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
} 