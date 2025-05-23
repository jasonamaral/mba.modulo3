using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class LessonCompleteRequest
{
    [Required(ErrorMessage = "The 'Completed' field is required.")]
    public bool Completed { get; set; }
    
    public int? Score { get; set; }
    
    public string? Feedback { get; set; }
} 