using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class RefundProcessRequest
{
    [Required(ErrorMessage = "Reason is required.")]
    [StringLength(200, ErrorMessage = "Reason cannot exceed 200 characters.")]
    public string Reason { get; set; } = string.Empty;
} 