using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class LessonCompleteRequest
{
    [Required(ErrorMessage = "O campo 'Completed' é obrigatório.")]
    public bool Completed { get; set; }
    
    public int? Score { get; set; }
    
    public string? Feedback { get; set; }
} 