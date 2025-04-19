using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "O e-mail deve ter entre 5 e 200 caracteres.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 50 caracteres.")]
    public required string Password { get; set; }
}