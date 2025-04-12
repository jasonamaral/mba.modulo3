using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class LessonCompleteRequest
{
    [Required]
    public bool Completed { get; set; }
} 