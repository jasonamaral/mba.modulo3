using FluencyHub.ContentManagement.Application.Commands.AddLesson;
using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class LessonCreateRequest
{
    [Required(ErrorMessage = "O título da lição é obrigatório.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "O título deve ter entre 3 e 200 caracteres.")]
    public required string Title { get; set; }

    [Required(ErrorMessage = "A descrição da lição é obrigatória.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "A descrição deve ter entre 10 e 500 caracteres.")]
    public required string Description { get; set; }

    [Required(ErrorMessage = "O conteúdo da lição é obrigatório.")]
    [StringLength(10000, MinimumLength = 10, ErrorMessage = "O conteúdo deve ter entre 10 e 10.000 caracteres.")]
    public required string Content { get; set; }

    [Url(ErrorMessage = "A URL do material deve ser válida.")]
    public string? VideoUrl { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A ordem deve ser maior ou igual a 1.")]
    public int Order { get; set; } = 1;
    
    [Range(0, int.MaxValue, ErrorMessage = "A duração deve ser maior ou igual a 0.")]
    public int DurationMinutes { get; set; } = 0;

    public AddLessonCommand ToCommand(Guid courseId)
    {
        return new AddLessonCommand
        {
            CourseId = courseId,
            Title = Title,
            Description = Description,
            Content = Content,
            Order = Order,
            DurationMinutes = DurationMinutes,
            VideoUrl = VideoUrl
        };
    }
}