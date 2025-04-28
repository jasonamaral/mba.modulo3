using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FluencyHub.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public IdentityService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<AuthResult> AuthenticateAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return AuthResult.Failure("Invalid email or password");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);

        if (!isPasswordValid)
        {
            return AuthResult.Failure("Invalid email or password");
        }

        var token = await GenerateJwtTokenAsync(user);
        return AuthResult.Success(token);
    }

    public async Task<AuthResult> RegisterUserAsync(string email, string password, string firstName, string lastName)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser != null)
        {
            return AuthResult.Failure("User with this email already exists");
        }

        var user = new ApplicationUser
        {
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return AuthResult.Failure(errors);
        }

        await _userManager.AddToRoleAsync(user, "Student");

        var token = await GenerateJwtTokenAsync(user);
        return AuthResult.Success(token);
    }

    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName)
        };

        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var secret = _configuration["JwtSettings:Secret"] ?? "ThisIsATemporarySecretForTestingPurposesOnly12345";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var issuer = _configuration["JwtSettings:Issuer"] ?? "TestIssuer";
        var audience = _configuration["JwtSettings:Audience"] ?? "TestAudience";
        var expiryDaysStr = _configuration["JwtSettings:ExpiryInDays"] ?? "7";
        var expiryDays = Convert.ToDouble(expiryDaysStr);

        var expires = DateTime.Now.AddDays(expiryDays);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<bool> UpdateUserStudentIdAsync(string email, Guid studentId)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return false;
        }

        user.StudentId = studentId;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> DeleteUserAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }
}