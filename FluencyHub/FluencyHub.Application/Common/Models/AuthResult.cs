namespace FluencyHub.Application.Common.Models;

public class AuthResult
{
    public bool Succeeded { get; private set; }
    public string? Token { get; private set; }
    public string[] Errors { get; private set; } = Array.Empty<string>();
    
    private AuthResult() { }
    
    public static AuthResult Success(string token)
    {
        return new AuthResult
        {
            Succeeded = true,
            Token = token
        };
    }
    
    public static AuthResult Failure(params string[] errors)
    {
        return new AuthResult
        {
            Succeeded = false,
            Errors = errors
        };
    }
} 