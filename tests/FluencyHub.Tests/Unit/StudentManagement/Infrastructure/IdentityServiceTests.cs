using FluencyHub.StudentManagement.Infrastructure.Identity;
using FluencyHub.StudentManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using FluentAssertions;

namespace FluencyHub.Tests.Unit.StudentManagement.Infrastructure;

public class IdentityServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly IdentityService _identityService;

    public IdentityServiceTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object, 
            Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(), 
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null!, null!, null!, null!);

        _configurationMock = new Mock<IConfiguration>();
        SetupJwtConfiguration();

        _identityService = new IdentityService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _configurationMock.Object);
    }

    private void SetupJwtConfiguration()
    {
        var jwtSectionMock = new Mock<IConfigurationSection>();
        jwtSectionMock.Setup(x => x["Secret"]).Returns("SuperSecretKeyForJwtTokenThatIsLongEnough");
        jwtSectionMock.Setup(x => x["Issuer"]).Returns("FluencyHub");
        jwtSectionMock.Setup(x => x["Audience"]).Returns("FluencyHub");
        jwtSectionMock.Setup(x => x["ExpiryInDays"]).Returns("7");

        _configurationMock.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSectionMock.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnSuccess_WhenValidCredentials()
    {
        // Arrange
        var email = "joao@test.com";
        var password = "Password123!";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            UserName = email,
            FirstName = "João",
            LastName = "Silva"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
            .ReturnsAsync(SignInResult.Success);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        // Act
        var result = await _identityService.AuthenticateAsync(email, password);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Token.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnFailure_WhenInvalidCredentials()
    {
        // Arrange
        var email = "joao@test.com";
        var password = "WrongPassword";

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _identityService.AuthenticateAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnFailure_WhenPasswordIsIncorrect()
    {
        // Arrange
        var email = "joao@test.com";
        var password = "WrongPassword";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            UserName = email,
            FirstName = "João",
            LastName = "Silva"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var result = await _identityService.AuthenticateAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldCreateUser_WhenValidData()
    {
        // Arrange
        var email = "joao@test.com";
        var password = "Password123!";
        var firstName = "João";
        var lastName = "Silva";

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>());

        // Act
        var result = await _identityService.RegisterUserAsync(email, password, firstName, lastName);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Token.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), password), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldReturnFailure_WhenEmailExists()
    {
        // Arrange
        var email = "joao@test.com";
        var password = "Password123!";
        var firstName = "João";
        var lastName = "Silva";
        var existingUser = new ApplicationUser { Email = email };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _identityService.RegisterUserAsync(email, password, firstName, lastName);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Token.Should().BeNull();
        result.Errors.Should().Contain("Usuário já existe com este email");
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldReturnFailure_WhenIdentityCreateFails()
    {
        // Arrange
        var email = "joao@test.com";
        var password = "Password123!";
        var firstName = "João";
        var lastName = "Silva";
        var identityErrors = new[]
        {
            new IdentityError { Description = "Password too weak" },
            new IdentityError { Description = "Email format invalid" }
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        var result = await _identityService.RegisterUserAsync(email, password, firstName, lastName);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Token.Should().BeNull();
        result.Errors.Should().Contain("Password too weak");
        result.Errors.Should().Contain("Email format invalid");
    }

    [Fact]
    public async Task GenerateJwtToken_ShouldReturnValidToken_WhenValidUser()
    {
        // Arrange
        var email = "joao@test.com";
        var password = "Password123!";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            UserName = email,
            FirstName = "João",
            LastName = "Silva",
            StudentId = Guid.NewGuid()
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
            .ReturnsAsync(SignInResult.Success);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Student" });

        // Act
        var result = await _identityService.AuthenticateAsync(email, password);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        
        // Verificar se o token contém as informações esperadas
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.Token);
        
        token.Claims.Should().Contain(c => c.Type == "email" && c.Value == email);
        token.Claims.Should().Contain(c => c.Type == "firstName" && c.Value == "João");
        token.Claims.Should().Contain(c => c.Type == "lastName" && c.Value == "Silva");
        token.Claims.Should().Contain(c => c.Type == "studentId" && c.Value == user.StudentId.ToString());
        token.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Student");
    }

    [Fact]
    public async Task UpdateUserStudentIdAsync_ShouldReturnTrue_WhenUserExists()
    {
        // Arrange
        var email = "joao@test.com";
        var studentId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            UserName = email,
            FirstName = "João",
            LastName = "Silva"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _identityService.UpdateUserStudentIdAsync(email, studentId);

        // Assert
        result.Should().BeTrue();
        user.StudentId.Should().Be(studentId);
        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task UpdateUserStudentIdAsync_ShouldReturnFalse_WhenUserNotExists()
    {
        // Arrange
        var email = "nonexistent@test.com";
        var studentId = Guid.NewGuid();

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _identityService.UpdateUserStudentIdAsync(email, studentId);

        // Assert
        result.Should().BeFalse();
        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserStudentIdAsync_ShouldReturnFalse_WhenUpdateFails()
    {
        // Arrange
        var email = "joao@test.com";
        var studentId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            UserName = email,
            FirstName = "João",
            LastName = "Silva"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed());

        // Act
        var result = await _identityService.UpdateUserStudentIdAsync(email, studentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnTrue_WhenUserExists()
    {
        // Arrange
        var email = "joao@test.com";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            UserName = email,
            FirstName = "João",
            LastName = "Silva"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _identityService.DeleteUserAsync(email);

        // Assert
        result.Should().BeTrue();
        _userManagerMock.Verify(x => x.DeleteAsync(user), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnFalse_WhenUserNotExists()
    {
        // Arrange
        var email = "nonexistent@test.com";

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _identityService.DeleteUserAsync(email);

        // Assert
        result.Should().BeFalse();
        _userManagerMock.Verify(x => x.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnFalse_WhenDeleteFails()
    {
        // Arrange
        var email = "joao@test.com";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            UserName = email,
            FirstName = "João",
            LastName = "Silva"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Failed());

        // Act
        var result = await _identityService.DeleteUserAsync(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddToRoleAsync_ShouldReturnTrue_WhenUserExistsAndRoleAdded()
    {
        // Arrange
        var email = "joao@test.com";
        var roleName = "Admin";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            UserName = email,
            FirstName = "João",
            LastName = "Silva"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.AddToRoleAsync(user, roleName))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _identityService.AddToRoleAsync(email, roleName);

        // Assert
        result.Should().BeTrue();
        _userManagerMock.Verify(x => x.AddToRoleAsync(user, roleName), Times.Once);
    }

    [Fact]
    public async Task AddToRoleAsync_ShouldReturnFalse_WhenUserNotExists()
    {
        // Arrange
        var email = "nonexistent@test.com";
        var roleName = "Admin";

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _identityService.AddToRoleAsync(email, roleName);

        // Assert
        result.Should().BeFalse();
        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddToRoleAsync_ShouldReturnFalse_WhenAddToRoleFails()
    {
        // Arrange
        var email = "joao@test.com";
        var roleName = "Admin";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            UserName = email,
            FirstName = "João",
            LastName = "Silva"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.AddToRoleAsync(user, roleName))
            .ReturnsAsync(IdentityResult.Failed());

        // Act
        var result = await _identityService.AddToRoleAsync(email, roleName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EnsureRoleExistsAsync_ShouldReturnTrue_WhenCalled()
    {
        // Arrange
        var roleName = "Student";

        // Act
        var result = await _identityService.EnsureRoleExistsAsync(roleName);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("invalid", "password")]
    [InlineData("email@test.com", "")]
    [InlineData("email@test.com", "invalid")]
    public async Task AuthenticateAsync_ShouldThrowArgumentException_WhenInvalidParameters(string email, string password)
    {
        // Act & Assert
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _identityService.AuthenticateAsync(email, password));
        }
        else
        {
            // Para parâmetros válidos mas que devem falhar na autenticação
            var result = await _identityService.AuthenticateAsync(email, password);
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task GenerateJwtToken_ShouldThrowException_WhenJwtSecretNotConfigured()
    {
        // Arrange
        var jwtSectionMock = new Mock<IConfigurationSection>();
        jwtSectionMock.Setup(x => x["Secret"]).Returns((string?)null);
        jwtSectionMock.Setup(x => x["Issuer"]).Returns("FluencyHub");
        jwtSectionMock.Setup(x => x["Audience"]).Returns("FluencyHub");
        jwtSectionMock.Setup(x => x["ExpiryInDays"]).Returns("7");

        _configurationMock.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSectionMock.Object);

        var identityService = new IdentityService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _configurationMock.Object);

        var email = "joao@test.com";
        var password = "Password123!";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            UserName = email,
            FirstName = "João",
            LastName = "Silva"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
            .ReturnsAsync(SignInResult.Success);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => identityService.AuthenticateAsync(email, password));
    }

    [Fact]
    public async Task GenerateJwtToken_ShouldThrowException_WhenJwtIssuerNotConfigured()
    {
        // Arrange
        var jwtSectionMock = new Mock<IConfigurationSection>();
        jwtSectionMock.Setup(x => x["Secret"]).Returns("SuperSecretKeyForJwtTokenThatIsLongEnough");
        jwtSectionMock.Setup(x => x["Issuer"]).Returns((string?)null);
        jwtSectionMock.Setup(x => x["Audience"]).Returns("FluencyHub");
        jwtSectionMock.Setup(x => x["ExpiryInDays"]).Returns("7");

        _configurationMock.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSectionMock.Object);

        var identityService = new IdentityService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _configurationMock.Object);

        var email = "joao@test.com";
        var password = "Password123!";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            UserName = email,
            FirstName = "João",
            LastName = "Silva"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
            .ReturnsAsync(SignInResult.Success);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => identityService.AuthenticateAsync(email, password));
    }

    [Fact]
    public async Task GenerateJwtToken_ShouldThrowException_WhenJwtAudienceNotConfigured()
    {
        // Arrange
        var jwtSectionMock = new Mock<IConfigurationSection>();
        jwtSectionMock.Setup(x => x["Secret"]).Returns("SuperSecretKeyForJwtTokenThatIsLongEnough");
        jwtSectionMock.Setup(x => x["Issuer"]).Returns("FluencyHub");
        jwtSectionMock.Setup(x => x["Audience"]).Returns((string?)null);
        jwtSectionMock.Setup(x => x["ExpiryInDays"]).Returns("7");

        _configurationMock.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSectionMock.Object);

        var identityService = new IdentityService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _configurationMock.Object);

        var email = "joao@test.com";
        var password = "Password123!";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            UserName = email,
            FirstName = "João",
            LastName = "Silva"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
            .ReturnsAsync(SignInResult.Success);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => identityService.AuthenticateAsync(email, password));
    }
} 