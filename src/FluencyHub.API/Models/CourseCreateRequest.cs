using FluencyHub.Application.ContentManagement.Commands.CreateCourse;
using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class CourseCreateRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(5000)]
    public string Syllabus { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string LearningObjectives { get; set; } = string.Empty;

    [StringLength(1000)]
    public string PreRequisites { get; set; } = string.Empty;

    [StringLength(1000)]
    public string TargetAudience { get; set; } = "Todos os estudantes";

    [Required]
    [StringLength(50)]
    public required string Language { get; set; }

    [Required]
    [StringLength(50)]
    public required string Level { get; set; }

    [Required]
    [Range(0, 10000)]
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