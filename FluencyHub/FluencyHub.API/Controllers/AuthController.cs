using FluencyHub.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;
    
    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _identityService.AuthenticateAsync(request.Email, request.Password);
        
        if (!result.Succeeded)
        {
            return Unauthorized(new { errors = result.Errors });
        }
        
        return Ok(new { token = result.Token });
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _identityService.RegisterUserAsync(
            request.Email, 
            request.Password, 
            request.FirstName, 
            request.LastName);
        
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(new { token = result.Token });
    }
}

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string Email, string Password, string FirstName, string LastName); 