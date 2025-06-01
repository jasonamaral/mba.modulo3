using FluencyHub.API.Models;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Application.Common.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using FluencyHub.Tests.Helpers;

namespace FluencyHub.Tests.Integration.Controllers;

public class AuthControllerTests : IntegrationTestBase
{
    // 178. AuthController_Register_ShouldReturnToken_WhenValidRequest
    [Fact]
    public async Task Register_ShouldReturnToken_WhenValidRequest()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            FirstName = "João",
            LastName = "Silva",
            Email = "joao.silva@email.com",
            Password = "ValidPassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("mock-jwt-token");
    }

    // 179. AuthController_Register_ShouldReturnBadRequest_WhenEmailExists
    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailExists()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            FirstName = "João",
            LastName = "Silva",
            Email = "existing@example.com", // Este email retorna erro no MockIdentityService
            Password = "ValidPassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // 180. AuthController_Login_WithValidCredentials_ShouldReturnOk
    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "ValidPassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("mock-jwt-token");
    }

    // 181. AuthController_Login_WithInvalidCredentials_ShouldReturnUnauthorized
    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "invalid@example.com", // Este email retorna erro no MockIdentityService
            Password = "wrongpassword"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithEmptyEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "",
            Password = "ValidPassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = ""
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
} 