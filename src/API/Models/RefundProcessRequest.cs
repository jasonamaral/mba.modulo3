using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class RefundProcessRequest
{
    [Required(ErrorMessage = "A razão é obrigatória.")]
    [StringLength(200, ErrorMessage = "A razão não pode exceder 200 caracteres.")]
    public string Reason { get; set; } = string.Empty;
} 