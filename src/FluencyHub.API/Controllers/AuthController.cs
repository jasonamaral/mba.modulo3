using FluencyHub.API.Models;
using FluencyHub.API.SwaggerExamples;
using FluencyHub.Application.Common.Interfaces;
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
    /// Authenticate a user and get a JWT token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token for authenticated user</returns>
    /// <response code="200">Returns the JWT token</response>
    /// <response code="401">If authentication fails</response>
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Login",
        Description = "Authenticates a user and returns a JWT token for subsequent API calls",
        OperationId = "Login",
        Tags = new[] { "Authentication" }
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
}