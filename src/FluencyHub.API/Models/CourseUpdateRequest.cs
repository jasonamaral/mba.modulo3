using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class CourseUpdateRequest
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Syllabus { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string LearningObjectives { get; set; } = string.Empty;

    [StringLength(1000)]
    public string PreRequisites { get; set; } = string.Empty;

    [StringLength(1000)]
    public string TargetAudience { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Language { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Level { get; set; } = string.Empty;

    [Required]
    [Range(0, 10000)]
    public decimal Price { get; set; }
} 