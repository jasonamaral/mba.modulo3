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

namespace FluencyHub.Tests.Integration.Controllers;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly Mock<IIdentityService> _mockIdentityService;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        _mockIdentityService = new Mock<IIdentityService>();
        
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove o serviço real e adiciona o mock
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IIdentityService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddSingleton(_mockIdentityService.Object);
            });
        });
        
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "ValidPassword123!"
        };

        var authResult = AuthResult.Success("valid-jwt-token");

        _mockIdentityService
            .Setup(x => x.AuthenticateAsync(loginRequest.Email, loginRequest.Password))
            .ReturnsAsync(authResult);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("valid-jwt-token");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "InvalidPassword"
        };

        var authResult = AuthResult.Failure("Invalid credentials");

        _mockIdentityService
            .Setup(x => x.AuthenticateAsync(loginRequest.Email, loginRequest.Password))
            .ReturnsAsync(authResult);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

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
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

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
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
} 