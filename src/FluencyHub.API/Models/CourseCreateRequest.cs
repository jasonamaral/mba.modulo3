using FluencyHub.Application.ContentManagement.Commands.CreateCourse;
using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class CourseCreateRequest
{
    [Required(ErrorMessage = "O nome do curso � obrigat�rio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descri��o do curso � obrigat�ria.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "A descri��o deve ter entre 10 e 2000 caracteres.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "O conte�do program�tico � obrigat�rio.")]
    [StringLength(5000, ErrorMessage = "O conte�do program�tico deve ter no m�ximo 5000 caracteres.")]
    public string Syllabus { get; set; } = string.Empty;

    [Required(ErrorMessage = "Os objetivos de aprendizagem s�o obrigat�rios.")]
    [StringLength(2000, ErrorMessage = "Os objetivos devem ter no m�ximo 2000 caracteres.")]
    public string LearningObjectives { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Os pr�-requisitos devem ter no m�ximo 1000 caracteres.")]
    public string PreRequisites { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "O p�blico-alvo deve ter no m�ximo 1000 caracteres.")]
    public string TargetAudience { get; set; } = "Todos os estudantes";

    [Required(ErrorMessage = "O idioma � obrigat�rio.")]
    [StringLength(50, ErrorMessage = "O idioma deve ter no m�ximo 50 caracteres.")]
    public required string Language { get; set; }

    [Required(ErrorMessage = "O n�vel do curso � obrigat�rio.")]
    [StringLength(50, ErrorMessage = "O n�vel deve ter no m�ximo 50 caracteres.")]
    public required string Level { get; set; }

    [Required(ErrorMessage = "O pre�o � obrigat�rio.")]
    [Range(0, 10000, ErrorMessage = "O pre�o deve estar entre 0 e 10.000.")]
    public decimal Price { get; set; }

    public CreateCourseCommand ToCommand()
    {
        return new CreateCourseCommand
        {
            Name = Name,
            Description = Description,
            Price = Price,
            Syllabus = Syllabus,
            LearningObjectives = LearningObjectives,
            PreRequisites = PreRequisites,
            TargetAudience = TargetAudience,
            Language = Language,
            Level = Level
        };
    }
}