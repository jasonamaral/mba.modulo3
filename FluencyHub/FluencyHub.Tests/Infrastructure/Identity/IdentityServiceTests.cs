using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.Common.Models;
using FluencyHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FluencyHub.Tests.Infrastructure.Identity;

public class IdentityServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly IdentityService _identityService;

    public IdentityServiceTests()
    {
        _mockUserManager = MockUserManager<ApplicationUser>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        // Set up configuration for JWT
        var mockJwtSection = new Mock<IConfigurationSection>();
        mockJwtSection.Setup(s => s["Secret"]).Returns("SuperSecretKeyForTesting12345SuperSecretKeyForTesting12345");
        mockJwtSection.Setup(s => s["Issuer"]).Returns("TestIssuer");
        mockJwtSection.Setup(s => s["Audience"]).Returns("TestAudience");
        mockJwtSection.Setup(s => s["ExpiryInDays"]).Returns("7");
        
        _mockConfiguration.Setup(c => c.GetSection("JwtSettings")).Returns(mockJwtSection.Object);
        
        _identityService = new IdentityService(
            _mockUserManager.Object,
            _mockConfiguration.Object
        );
    }

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ShouldReturnSuccessWithToken()
    {
        // Arrange
        string email = "test@example.com";
        string password = "Password123!";
        
        var user = new ApplicationUser
        {
            Email = email,
            UserName = email,
            FirstName = "Test",
            LastName = "User"
        };
        
        _mockUserManager.Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.CheckPasswordAsync(user, password))
            .ReturnsAsync(true);
        _mockUserManager.Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Student" });

        // Act
        var result = await _identityService.AuthenticateAsync(email, password);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Token);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task AuthenticateAsync_InvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        string email = "nonexistent@example.com";
        string password = "Password123!";
        
        _mockUserManager.Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _identityService.AuthenticateAsync(email, password);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Token);
        Assert.Contains("Invalid email or password", result.Errors);
    }

    [Fact]
    public async Task AuthenticateAsync_InvalidPassword_ShouldReturnFailure()
    {
        // Arrange
        string email = "test@example.com";
        string password = "WrongPassword";
        
        var user = new ApplicationUser
        {
            Email = email,
            UserName = email
        };
        
        _mockUserManager.Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.CheckPasswordAsync(user, password))
            .ReturnsAsync(false);

        // Act
        var result = await _identityService.AuthenticateAsync(email, password);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Token);
        Assert.Contains("Invalid email or password", result.Errors);
    }

    [Fact]
    public async Task RegisterUserAsync_ValidData_ShouldRegisterUserAndReturnSuccess()
    {
        // Arrange
        string email = "newuser@example.com";
        string password = "Password123!";
        string firstName = "New";
        string lastName = "User";
        
        _mockUserManager.Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser)null);
        
        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(IdentityResult.Success);
        
        _mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Student"))
            .ReturnsAsync(IdentityResult.Success);
        
        _mockUserManager.Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "Student" });

        // Act
        var result = await _identityService.RegisterUserAsync(email, password, firstName, lastName);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Token);
        Assert.Empty(result.Errors);
        
        _mockUserManager.Verify(um => um.CreateAsync(
            It.Is<ApplicationUser>(u => 
                u.Email == email && 
                u.UserName == email && 
                u.FirstName == firstName && 
                u.LastName == lastName), 
            password), Times.Once);
        
        _mockUserManager.Verify(um => um.AddToRoleAsync(
            It.IsAny<ApplicationUser>(), 
            "Student"), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_DuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        string email = "existing@example.com";
        string password = "Password123!";
        string firstName = "Existing";
        string lastName = "User";
        
        var existingUser = new ApplicationUser
        {
            Email = email,
            UserName = email
        };
        
        _mockUserManager.Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _identityService.RegisterUserAsync(email, password, firstName, lastName);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Token);
        Assert.Contains("User with this email already exists", result.Errors);
        
        _mockUserManager.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterUserAsync_IdentityFailure_ShouldReturnFailureWithErrors()
    {
        // Arrange
        string email = "newuser@example.com";
        string password = "weak";
        string firstName = "New";
        string lastName = "User";
        
        _mockUserManager.Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser)null);
        
        var identityErrors = new List<IdentityError>
        {
            new IdentityError { Code = "PasswordTooShort", Description = "Password is too short" },
            new IdentityError { Code = "PasswordRequiresNonAlphanumeric", Description = "Password requires non-alphanumeric characters" }
        };
        
        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

        // Act
        var result = await _identityService.RegisterUserAsync(email, password, firstName, lastName);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Token);
        Assert.Equal(2, result.Errors.Length);
        Assert.Contains("Password is too short", result.Errors);
        Assert.Contains("Password requires non-alphanumeric characters", result.Errors);
    }

    // Helper method to mock UserManager
    private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        mgr.Object.UserValidators.Add(new UserValidator<TUser>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());
        return mgr;
    }
} 