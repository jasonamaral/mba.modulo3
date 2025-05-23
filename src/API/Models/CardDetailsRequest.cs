using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class CardDetailsRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string CardholderName { get; set; } = string.Empty;

    [Required]
    [StringLength(16, MinimumLength = 13)]
    [RegularExpression("^[0-9]+$", ErrorMessage = "Número do cartão deve conter apenas dígitos")]
    public string CardNumber { get; set; } = string.Empty;

    [Required]
    [Range(1, 12, ErrorMessage = "Mês de validade deve estar entre 1 e 12")]
    public int ExpiryMonth { get; set; }

    [Required]
    [Range(2000, 2100, ErrorMessage = "Ano de validade deve ser um ano válido")]
    public int ExpiryYear { get; set; }

    [Required]
    [StringLength(4, MinimumLength = 3)]
    [RegularExpression("^[0-9]+$", ErrorMessage = "CVV deve conter apenas dígitos")]
    public string Cvv { get; set; } = string.Empty;
}