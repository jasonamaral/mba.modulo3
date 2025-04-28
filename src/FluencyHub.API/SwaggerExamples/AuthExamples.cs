using FluencyHub.API.Models;
using Swashbuckle.AspNetCore.Filters;

namespace FluencyHub.API.SwaggerExamples;

public class LoginRequestExample : IExamplesProvider<LoginRequest>
{
    public LoginRequest GetExamples()
    {
        return new LoginRequest
        {
            Email = "student@example.com",
            Password = "Password123!"
        };
    }
}

public class LoginResponseExample : IExamplesProvider<LoginResponse>
{
    public LoginResponse GetExamples()
    {
        return new LoginResponse
        {
            Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"
        };
    }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
} 