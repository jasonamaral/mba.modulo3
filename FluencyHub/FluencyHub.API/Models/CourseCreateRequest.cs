using FluencyHub.Application.ContentManagement.Commands.CreateCourse;
using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class CourseCreateRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; }

    [Range(0, 9999.99)]
    public decimal Price { get; set; }

    [Required]
    public CourseContentRequest Content { get; set; }

    public CreateCourseCommand ToCommand()
    {
        return new CreateCourseCommand
        {
            Name = Name,
            Description = Description,
            Price = Price,
            Syllabus = Content.Description ?? string.Empty,
            LearningObjectives = Content.Goals ?? string.Empty,
            PreRequisites = Content.Requirements ?? string.Empty,
            TargetAudience = "Todos os estudantes", // Valor padrão
            Language = "Português", // Valor padrão
            Level = "Iniciante" // Valor padrão
        };
    }
}

public class CourseContentRequest
{
    public string Description { get; set; }
    public string Goals { get; set; }
    public string Requirements { get; set; }
} 