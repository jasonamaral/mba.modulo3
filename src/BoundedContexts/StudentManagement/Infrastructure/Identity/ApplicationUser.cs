using Microsoft.AspNetCore.Identity;

namespace FluencyHub.StudentManagement.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid? StudentId { get; set; }
} 