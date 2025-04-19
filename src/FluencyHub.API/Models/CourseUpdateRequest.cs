using System.ComponentModel.DataAnnotations;
using FluencyHub.Application.ContentManagement.Commands.UpdateCourse;

namespace FluencyHub.API.Models;

public class CourseUpdateRequest
{
    [Required(ErrorMessage = "O ID do curso é obrigatório.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "O nome do curso é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição do curso é obrigatória.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "A descrição deve ter entre 10 e 2000 caracteres.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "O conteúdo programático é obrigatório.")]
    [StringLength(5000, ErrorMessage = "O conteúdo programático deve ter no máximo 5000 caracteres.")]
    public string Syllabus { get; set; } = string.Empty;

    [Required(ErrorMessage = "Os objetivos de aprendizagem são obrigatórios.")]
    [StringLength(2000, ErrorMessage = "Os objetivos devem ter no máximo 2000 caracteres.")]
    public string LearningObjectives { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Os pré-requisitos devem ter no máximo 1000 caracteres.")]
    public string PreRequisites { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "O público-alvo deve ter no máximo 1000 caracteres.")]
    public string TargetAudience { get; set; } = string.Empty;

    [Required(ErrorMessage = "O idioma é obrigatório.")]
    [StringLength(50, ErrorMessage = "O idioma deve ter no máximo 50 caracteres.")]
    public string Language { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nível do curso é obrigatório.")]
    [StringLength(50, ErrorMessage = "O nível deve ter no máximo 50 caracteres.")]
    public string Level { get; set; } = string.Empty;

    [Required(ErrorMessage = "O preço é obrigatório.")]
    [Range(0, 10000, ErrorMessage = "O preço deve estar entre 0 e 10.000.")]
    public decimal Price { get; set; }

    public UpdateCourseCommand ToCommand()
    {
        return new UpdateCourseCommand
        {
            Id = Id,
            Name = Name,
            Description = Description,
            Syllabus = Syllabus,
            LearningObjectives = LearningObjectives,
            PreRequisites = PreRequisites,
            TargetAudience = TargetAudience,
            Language = Language,
            Level = Level,
            Price = Price
        };
    }
}