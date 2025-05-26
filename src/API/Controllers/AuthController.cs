using FluencyHub.API.Models;
using FluencyHub.API.SwaggerExamples;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Net.Mime;

namespace FluencyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    /// <summary>
    /// Autentique um usuário e obtenha um token JWT
    /// </summary>
    /// <param name="request">Credenciais de login</param>
    /// <returns>Token JWT para usuário autenticado</returns>
    /// <response code="200">Retorna o token JWT</response>
    /// <response code="401">Se a autenticação falhar</response>
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Login",
        Description = "Autentica um usuário e retorna um token JWT para chamadas de API subsequentes",
        OperationId = "Login"
    )]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerRequestExample(typeof(LoginRequest), typeof(LoginRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(LoginResponseExample))]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _identityService.AuthenticateAsync(request.Email, request.Password);

        if (!result.Succeeded)
        {
            return Unauthorized(new { errors = result.Errors });
        }

        return Ok(new { token = result.Token });
    }

    /// <summary>
    /// Registre um novo usuário
    /// </summary>
    /// <param name="request">Dados do novo usuário</param>
    /// <returns>Token JWT para usuário registrado</returns>
    /// <response code="200">Retorna o token JWT</response>
    /// <response code="400">Se o registro falhar</response>
    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "Registro",
        Description = "Registra um novo usuário e retorna um token JWT",
        OperationId = "Register"
    )]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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