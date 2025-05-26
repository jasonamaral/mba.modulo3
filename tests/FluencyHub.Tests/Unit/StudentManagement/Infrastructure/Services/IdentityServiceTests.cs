using FluencyHub.StudentManagement.Application.Common.Models;
using FluencyHub.StudentManagement.Infrastructure.Identity;
using FluencyHub.StudentManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using Xunit;
using FluentAssertions;

namespace FluencyHub.Tests.Unit.StudentManagement.Infrastructure.Services;

public class IdentityServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IConfigurationSection> _mockJwtSection;
    private readonly IdentityService _identityService;

    public IdentityServiceTests()
    {
        // Setup UserManager mock
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null, null, null, null, null, null, null, null);

        // Setup SignInManager mock
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            _mockUserManager.Object, contextAccessor.Object, claimsFactory.Object, null, null, null, null);

        // Setup Configuration mock
        _mockConfiguration = new Mock<IConfiguration>();
        _mockJwtSection = new Mock<IConfigurationSection>();
        
        SetupJwtConfiguration();

        _identityService = new IdentityService(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockConfiguration.Object);
    }

    private void SetupJwtConfiguration()
    {
        _mockJwtSection.Setup(x => x["Secret"]).Returns("YourSuperSecretKeyThatShouldBeAtLeast256BitsLongForTestingPurposes");
        _mockJwtSection.Setup(x => x["Issuer"]).Returns("FluencyHub");
        _mockJwtSection.Setup(x => x["Audience"]).Returns("FluencyHubClients");
        _mockJwtSection.Setup(x => x["ExpiryInDays"]).Returns("7");

        _mockConfiguration.Setup(x => x.GetSection("JwtSettings")).Returns(_mockJwtSection.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnSuccessWithToken()
    {
        // Arrange
        var email = "test@example.com";
        var password = "ValidPassword123!";
        var user = new ApplicationUser
        {
            Id = "user-id",
            Email = email,
            UserName = email,
            FirstName = "Test",
            LastName = "User"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
            .ReturnsAsync(SignInResult.Success);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

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
        token.Claims.Should().Contain(c => c.Type == "firstName" && c.Value == "Test");
        token.Claims.Should().Contain(c => c.Type == "lastName" && c.Value == "User");
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var password = "ValidPassword123!";

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _identityService.AuthenticateAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Token.Should().BeNull();
        result.Errors.Should().Contain("Credenciais inválidas");
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidPassword_ShouldReturnFailure()
    {
        // Arrange
        var email = "test@example.com";
        var password = "InvalidPassword";
        var user = new ApplicationUser
        {
            Id = "user-id",
            Email = email,
            UserName = email,
            FirstName = "Test",
            LastName = "User"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var result = await _identityService.AuthenticateAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Token.Should().BeNull();
        result.Errors.Should().Contain("Credenciais inválidas");
    }

    [Fact]
    public async Task AuthenticateAsync_WithUserHavingStudentId_ShouldIncludeStudentIdInToken()
    {
        // Arrange
        var email = "test@example.com";
        var password = "ValidPassword123!";
        var studentId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = "user-id",
            Email = email,
            UserName = email,
            FirstName = "Test",
            LastName = "User",
            StudentId = studentId
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
            .ReturnsAsync(SignInResult.Success);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

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
    public async Task RegisterUserAsync_WithValidData_ShouldReturnSuccessWithToken()
    {
        // Arrange
        var email = "newuser@example.com";
        var password = "ValidPassword123!";
        var firstName = "New";
        var lastName = "User";

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>());

        // Act
        var result = await _identityService.RegisterUserAsync(email, password, firstName, lastName);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.Errors.Should().BeEmpty();

        // Verify user creation was called with correct data
        _mockUserManager.Verify(x => x.CreateAsync(
            It.Is<ApplicationUser>(u => 
                u.Email == email && 
                u.UserName == email && 
                u.FirstName == firstName && 
                u.LastName == lastName), 
            password), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var email = "existing@example.com";
        var password = "ValidPassword123!";
        var firstName = "Test";
        var lastName = "User";
        var existingUser = new ApplicationUser { Email = email };

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _identityService.RegisterUserAsync(email, password, firstName, lastName);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Token.Should().BeNull();
        result.Errors.Should().Contain("Usuário já existe com este email");

        // Verify CreateAsync was not called
        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterUserAsync_WithIdentityErrors_ShouldReturnFailureWithErrors()
    {
        // Arrange
        var email = "newuser@example.com";
        var password = "weak";
        var firstName = "New";
        var lastName = "User";

        var identityErrors = new[]
        {
            new IdentityError { Description = "Password too weak" },
            new IdentityError { Description = "Password must contain uppercase letter" }
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        var result = await _identityService.RegisterUserAsync(email, password, firstName, lastName);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Token.Should().BeNull();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain("Password too weak");
        result.Errors.Should().Contain("Password must contain uppercase letter");
    }

    [Fact]
    public async Task UpdateUserStudentIdAsync_WithValidEmail_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        var studentId = Guid.NewGuid();
        var user = new ApplicationUser { Email = email };

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _identityService.UpdateUserStudentIdAsync(email, studentId);

        // Assert
        result.Should().BeTrue();
        user.StudentId.Should().Be(studentId);
        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task UpdateUserStudentIdAsync_WithInvalidEmail_ShouldReturnFalse()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var studentId = Guid.NewGuid();

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _identityService.UpdateUserStudentIdAsync(email, studentId);

        // Assert
        result.Should().BeFalse();
        _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserStudentIdAsync_WithUpdateFailure_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var studentId = Guid.NewGuid();
        var user = new ApplicationUser { Email = email };

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed());

        // Act
        var result = await _identityService.UpdateUserStudentIdAsync(email, studentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteUserAsync_WithValidEmail_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        var user = new ApplicationUser { Email = email };

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _identityService.DeleteUserAsync(email);

        // Assert
        result.Should().BeTrue();
        _mockUserManager.Verify(x => x.DeleteAsync(user), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WithInvalidEmail_ShouldReturnFalse()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _identityService.DeleteUserAsync(email);

        // Assert
        result.Should().BeFalse();
        _mockUserManager.Verify(x => x.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUserAsync_WithDeleteFailure_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var user = new ApplicationUser { Email = email };

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Failed());

        // Act
        var result = await _identityService.DeleteUserAsync(email);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task AuthenticateAsync_WithInvalidEmail_ShouldThrowArgumentException(string invalidEmail)
    {
        // Arrange
        var password = "ValidPassword123!";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _identityService.AuthenticateAsync(invalidEmail, password));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task AuthenticateAsync_WithInvalidPassword_ShouldThrowArgumentException(string invalidPassword)
    {
        // Arrange
        var email = "test@example.com";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _identityService.AuthenticateAsync(email, invalidPassword));
    }

    [Fact]
    public async Task AuthenticateAsync_WithUserHavingMultipleRoles_ShouldIncludeAllRolesInToken()
    {
        // Arrange
        var email = "admin@example.com";
        var password = "ValidPassword123!";
        var user = new ApplicationUser
        {
            Id = "admin-id",
            Email = email,
            UserName = email,
            FirstName = "Admin",
            LastName = "User"
        };

        var roles = new List<string> { "Admin", "User", "Manager" };

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
            .ReturnsAsync(SignInResult.Success);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        // Act
        var result = await _identityService.AuthenticateAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();

        // Verify JWT token contains all roles
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.Token);
        
        foreach (var role in roles)
        {
            token.Claims.Should().Contain(c => c.Type == "role" && c.Value == role);
        }
    }

    [Fact]
    public void Constructor_WithNullUserManager_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new IdentityService(null!, _mockSignInManager.Object, _mockConfiguration.Object));
    }

    [Fact]
    public void Constructor_WithNullSignInManager_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new IdentityService(_mockUserManager.Object, null!, _mockConfiguration.Object));
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new IdentityService(_mockUserManager.Object, _mockSignInManager.Object, null!));
    }
} 