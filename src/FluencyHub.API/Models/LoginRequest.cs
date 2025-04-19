using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "The provided email is not valid.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Email must be between 5 and 200 characters.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 50 characters.")]
    public required string Password { get; set; }
}