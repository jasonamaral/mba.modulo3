using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "Email é obrigatório.")]
    [EmailAddress(ErrorMessage = "O email fornecido não é válido.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Email deve ter entre 5 e 200 caracteres.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Senha é obrigatória.")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Senha deve ter entre 8 e 50 caracteres.")]
    public required string Password { get; set; }
}