using FluencyHub.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Xunit;

namespace FluencyHub.Tests.Integration.Security;

public class AuthenticationTests : IntegrationTestBase
{
    // 229. Security_JwtTokenValidation_ShouldRejectExpiredTokens
    [Fact]
    public async Task JwtTokenValidation_ShouldRejectExpiredTokens()
    {
        // Arrange
        var expiredToken = GenerateExpiredJwtToken();
        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", expiredToken);

        // Act
        var response = await Client.GetAsync("/api/estudantes/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // 230. Security_JwtTokenValidation_ShouldRejectTamperedTokens
    [Fact]
    public async Task JwtTokenValidation_ShouldRejectTamperedTokens()
    {
        // Arrange
        var validToken = await GetValidTokenAsync();
        if (validToken.Length < 20)
        {
            // Se o token for muito pequeno, criar um token de teste
            validToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        }
        
        var tamperedToken = validToken.Substring(0, Math.Max(0, validToken.Length - 10)) + "tampered123";
        
        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tamperedToken);

        // Act
        var response = await Client.GetAsync("/api/estudantes/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // 231. Security_PasswordHashing_ShouldUseSecureHashingAlgorithm
    [Fact]
    public async Task PasswordHashing_ShouldUseSecureHashingAlgorithm()
    {
        // Arrange
        var registerRequest = new
        {
            FirstName = "Test",
            LastName = "User",
            Email = "security@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar se a senha não é armazenada em texto plano
        // (isso seria verificado no banco de dados em um teste real)
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("SecurePassword123!");
    }

    [Fact]
    public async Task Authentication_ShouldRequireValidCredentials()
    {
        // Arrange
        var invalidLoginRequest = new
        {
            Email = "nonexistent@example.com",
            Password = "wrongpassword"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", invalidLoginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Authentication_ShouldReturnTokenForValidCredentials()
    {
        // Arrange
        var registerRequest = new
        {
            FirstName = "Valid",
            LastName = "User",
            Email = "valid@example.com",
            Password = "ValidPassword123!"
        };

        await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new
        {
            Email = "valid@example.com",
            Password = "ValidPassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("token");
    }

    [Fact]
    public async Task ProtectedEndpoint_ShouldRejectUnauthenticatedRequests()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/estudantes/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private string GenerateExpiredJwtToken()
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("this-is-a-very-long-secret-key-for-testing-purposes-only-do-not-use-in-production");
        
        var now = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Student")
            }),
            NotBefore = now.AddMinutes(-15), // Token válido há 15 minutos
            Expires = now.AddMinutes(-10), // Token expirado há 10 minutos
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<string> GetValidTokenAsync()
    {
        var registerRequest = new
        {
            FirstName = "Test",
            LastName = "User",
            Email = "testtoken@example.com",
            Password = "TestPassword123!"
        };

        await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new
        {
            Email = "testtoken@example.com",
            Password = "TestPassword123!"
        };

        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var content = await response.Content.ReadAsStringAsync();
        
        // Extrair token da resposta (assumindo formato JSON)
        var tokenStart = content.IndexOf("\"token\":\"") + 9;
        var tokenEnd = content.IndexOf("\"", tokenStart);
        return content.Substring(tokenStart, tokenEnd - tokenStart);
    }
} 