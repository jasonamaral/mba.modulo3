using FluencyHub.Application.Common.Models;

namespace FluencyHub.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthResult> AuthenticateAsync(string email, string password);
    Task<AuthResult> RegisterUserAsync(string email, string password, string firstName, string lastName);
    Task<bool> UpdateUserStudentIdAsync(string email, Guid studentId);
} 