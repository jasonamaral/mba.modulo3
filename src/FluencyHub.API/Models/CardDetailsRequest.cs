using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class CardDetailsRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string CardholderName { get; set; } = string.Empty;

    [Required]
    [StringLength(16, MinimumLength = 13)]
    [RegularExpression("^[0-9]+$", ErrorMessage = "Card number must contain only digits")]
    public string CardNumber { get; set; } = string.Empty;

    [Required]
    [Range(1, 12, ErrorMessage = "Expiry month must be between 1 and 12")]
    public int ExpiryMonth { get; set; }

    [Required]
    [Range(2000, 2100, ErrorMessage = "Expiry year must be a valid year")]
    public int ExpiryYear { get; set; }

    [Required]
    [StringLength(4, MinimumLength = 3)]
    [RegularExpression("^[0-9]+$", ErrorMessage = "CVV must contain only digits")]
    public string Cvv { get; set; } = string.Empty;
}