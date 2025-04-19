using FluencyHub.Application.ContentManagement.Commands.AddLesson;
using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class LessonCreateRequest
{
    [Required(ErrorMessage = "O título da aula é obrigatório.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "O título deve ter entre 3 e 200 caracteres.")]
    public required string Title { get; set; }

    [Required(ErrorMessage = "O conteúdo da aula é obrigatório.")]
    [StringLength(10000, MinimumLength = 10, ErrorMessage = "O conteúdo deve ter entre 10 e 10.000 caracteres.")]
    public required string Content { get; set; }

    [Url(ErrorMessage = "A URL do material deve ser válida.")]
    public string? MaterialUrl { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A ordem deve ser maior ou igual a 1.")]
    public int Order { get; set; } = 1;

    public AddLessonCommand ToCommand(Guid courseId)
    {
        return new AddLessonCommand
        {
            CourseId = courseId,
            Title = Title,
            Content = Content,
            MaterialUrl = MaterialUrl
        };
    }
}