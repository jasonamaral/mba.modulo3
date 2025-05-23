using FluencyHub.ContentManagement.Application.Commands.CreateCourse;
using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class CourseCreateRequest
{
    [Required(ErrorMessage = "Course name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Course description is required.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Course syllabus is required.")]
    [StringLength(5000, ErrorMessage = "Syllabus must be at most 5000 characters.")]
    public string Syllabus { get; set; } = string.Empty;

    [Required(ErrorMessage = "Learning objectives are required.")]
    [StringLength(2000, ErrorMessage = "Learning objectives must be at most 2000 characters.")]
    public string LearningObjectives { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Prerequisites must be at most 1000 characters.")]
    public string PreRequisites { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Target audience must be at most 1000 characters.")]
    public string TargetAudience { get; set; } = "All students";

    [Required(ErrorMessage = "Language is required.")]
    [StringLength(50, ErrorMessage = "Language must be at most 50 characters.")]
    public required string Language { get; set; }

    [Required(ErrorMessage = "Course level is required.")]
    [StringLength(50, ErrorMessage = "Level must be at most 50 characters.")]
    public required string Level { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0, 10000, ErrorMessage = "Price must be between 0 and 10,000.")]
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