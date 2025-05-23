using FluencyHub.ContentManagement.Application.Common.Models;

namespace FluencyHub.ContentManagement.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthResult> AuthenticateAsync(string email, string password);
    Task<AuthResult> RegisterUserAsync(string email, string password, string firstName, string lastName);
    Task<bool> UpdateUserStudentIdAsync(string email, Guid studentId);
    Task<bool> DeleteUserAsync(string email);
} 