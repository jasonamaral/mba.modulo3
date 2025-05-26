using FluencyHub.StudentManagement.Infrastructure.Identity;
using FluencyHub.StudentManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using Xunit;
using FluentAssertions;

namespace FluencyHub.Tests.Integration.Services;

public class IdentityServiceIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _context;
    private readonly IdentityService _identityService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityServiceIntegrationTests()
    {
        var services = new ServiceCollection();

        // Configure in-memory database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Configure Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Relaxed password requirements for testing
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 3;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Configure test configuration
        var configurationData = new Dictionary<string, string>
        {
            {"JwtSettings:Secret", "YourSuperSecretKeyThatShouldBeAtLeast256BitsLongForTestingPurposes"},
            {"JwtSettings:Issuer", "FluencyHub"},
            {"JwtSettings:Audience", "FluencyHubClients"},
            {"JwtSettings:ExpiryInDays", "7"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData!)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        // Register IdentityService
        services.AddScoped<IdentityService>();

        _serviceProvider = services.BuildServiceProvider();

        // Get services
        _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        _userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        _identityService = _serviceProvider.GetRequiredService<IdentityService>();

        // Ensure database is created
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task RegisterUserAsync_WithValidData_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        var email = "integration@test.com";
        var password = "TestPassword123!";
        var firstName = "Integration";
        var lastName = "Test";

        // Act
        var result = await _identityService.RegisterUserAsync(email, password, firstName, lastName);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.Errors.Should().BeEmpty();

        // Verify user was created in database
        var user = await _userManager.FindByEmailAsync(email);
        user.Should().NotBeNull();
        user!.Email.Should().Be(email);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);

        // Verify JWT token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.Token);
        token.Claims.Should().Contain(c => c.Type == "email" && c.Value == email);
        token.Claims.Should().Contain(c => c.Type == "firstName" && c.Value == firstName);
        token.Claims.Should().Contain(c => c.Type == "lastName" && c.Value == lastName);
    }

    [Fact]
    public async Task AuthenticateAsync_WithRegisteredUser_ShouldReturnValidToken()
    {
        // Arrange
        var email = "auth@test.com";
        var password = "TestPassword123!";
        var firstName = "Auth";
        var lastName = "Test";

        // First register the user
        await _identityService.RegisterUserAsync(email, password, firstName, lastName);

        // Act
        var result = await _identityService.AuthenticateAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.Errors.Should().BeEmpty();

        // Verify JWT token structure
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.Token);
        token.Claims.Should().Contain(c => c.Type == "email" && c.Value == email);
        token.Claims.Should().Contain(c => c.Type == "firstName" && c.Value == firstName);
        token.Claims.Should().Contain(c => c.Type == "lastName" && c.Value == lastName);
    }

    [Fact]
    public async Task UpdateUserStudentIdAsync_WithExistingUser_ShouldUpdateSuccessfully()
    {
        // Arrange
        var email = "update@test.com";
        var password = "TestPassword123!";
        var firstName = "Update";
        var lastName = "Test";
        var studentId = Guid.NewGuid();

        // Register user first
        await _identityService.RegisterUserAsync(email, password, firstName, lastName);

        // Act
        var result = await _identityService.UpdateUserStudentIdAsync(email, studentId);

        // Assert
        result.Should().BeTrue();

        // Verify in database
        var user = await _userManager.FindByEmailAsync(email);
        user.Should().NotBeNull();
        user!.StudentId.Should().Be(studentId);
    }

    [Fact]
    public async Task AuthenticateAsync_WithUserHavingStudentId_ShouldIncludeStudentIdInToken()
    {
        // Arrange
        var email = "studentid@test.com";
        var password = "TestPassword123!";
        var firstName = "StudentId";
        var lastName = "Test";
        var studentId = Guid.NewGuid();

        // Register user and update student ID
        await _identityService.RegisterUserAsync(email, password, firstName, lastName);
        await _identityService.UpdateUserStudentIdAsync(email, studentId);

        // Act
        var result = await _identityService.AuthenticateAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();

        // Verify JWT token contains studentId
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.Token);
        token.Claims.Should().Contain(c => c.Type == "studentId" && c.Value == studentId.ToString());
    }

    [Fact]
    public async Task DeleteUserAsync_WithExistingUser_ShouldDeleteSuccessfully()
    {
        // Arrange
        var email = "delete@test.com";
        var password = "TestPassword123!";
        var firstName = "Delete";
        var lastName = "Test";

        // Register user first
        await _identityService.RegisterUserAsync(email, password, firstName, lastName);

        // Verify user exists
        var userBefore = await _userManager.FindByEmailAsync(email);
        userBefore.Should().NotBeNull();

        // Act
        var result = await _identityService.DeleteUserAsync(email);

        // Assert
        result.Should().BeTrue();

        // Verify user was deleted
        var userAfter = await _userManager.FindByEmailAsync(email);
        userAfter.Should().BeNull();
    }

    [Fact]
    public async Task RegisterUserAsync_WithDuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        var email = "duplicate@test.com";
        var password = "TestPassword123!";
        var firstName = "Duplicate";
        var lastName = "Test";

        // Register user first time
        await _identityService.RegisterUserAsync(email, password, firstName, lastName);

        // Act - try to register again with same email
        var result = await _identityService.RegisterUserAsync(email, password, "Another", "User");

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Token.Should().BeNull();
        result.Errors.Should().Contain("Usuário já existe com este email");
    }

    [Fact]
    public async Task AuthenticateAsync_WithWrongPassword_ShouldReturnFailure()
    {
        // Arrange
        var email = "wrongpass@test.com";
        var password = "CorrectPassword123!";
        var wrongPassword = "WrongPassword123!";
        var firstName = "Wrong";
        var lastName = "Password";

        // Register user first
        await _identityService.RegisterUserAsync(email, password, firstName, lastName);

        // Act
        var result = await _identityService.AuthenticateAsync(email, wrongPassword);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Token.Should().BeNull();
        result.Errors.Should().Contain("Credenciais inválidas");
    }

    [Fact]
    public async Task AuthenticateAsync_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var email = "nonexistent@test.com";
        var password = "TestPassword123!";

        // Act
        var result = await _identityService.AuthenticateAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Token.Should().BeNull();
        result.Errors.Should().Contain("Credenciais inválidas");
    }

    [Fact]
    public async Task RegisterUserAsync_WithWeakPassword_ShouldReturnFailureWithErrors()
    {
        // Arrange
        var email = "weakpass@test.com";
        var weakPassword = "123"; // Too short
        var firstName = "Weak";
        var lastName = "Password";

        // Temporarily change password requirements to be more strict
        var userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var options = _serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<IdentityOptions>>();
        options.Value.Password.RequiredLength = 6;
        options.Value.Password.RequireDigit = true;
        options.Value.Password.RequireUppercase = true;

        // Act
        var result = await _identityService.RegisterUserAsync(email, weakPassword, firstName, lastName);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Token.Should().BeNull();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task FullUserLifecycle_ShouldWorkCorrectly()
    {
        // Arrange
        var email = "lifecycle@test.com";
        var password = "TestPassword123!";
        var firstName = "Lifecycle";
        var lastName = "Test";
        var studentId = Guid.NewGuid();

        // Act & Assert - Register
        var registerResult = await _identityService.RegisterUserAsync(email, password, firstName, lastName);
        registerResult.Succeeded.Should().BeTrue();

        // Act & Assert - Authenticate
        var authResult = await _identityService.AuthenticateAsync(email, password);
        authResult.Succeeded.Should().BeTrue();

        // Act & Assert - Update Student ID
        var updateResult = await _identityService.UpdateUserStudentIdAsync(email, studentId);
        updateResult.Should().BeTrue();

        // Act & Assert - Authenticate with Student ID
        var authWithStudentIdResult = await _identityService.AuthenticateAsync(email, password);
        authWithStudentIdResult.Succeeded.Should().BeTrue();
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(authWithStudentIdResult.Token!);
        token.Claims.Should().Contain(c => c.Type == "studentId" && c.Value == studentId.ToString());

        // Act & Assert - Delete
        var deleteResult = await _identityService.DeleteUserAsync(email);
        deleteResult.Should().BeTrue();

        // Act & Assert - Try to authenticate after deletion
        var authAfterDeleteResult = await _identityService.AuthenticateAsync(email, password);
        authAfterDeleteResult.Succeeded.Should().BeFalse();
    }

    public void Dispose()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }
} 