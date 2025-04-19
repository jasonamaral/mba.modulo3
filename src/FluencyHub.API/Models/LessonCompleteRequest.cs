using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class LessonCompleteRequest
{
    [Required(ErrorMessage = "O campo 'Completed' � obrigat�rio.")]
    public bool Completed { get; set; }
} 