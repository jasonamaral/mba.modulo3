using FluencyHub.API.Controllers;
using FluencyHub.API.Models;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Text.Json;

namespace FluencyHub.Tests.API.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _controller = new AuthController(_identityServiceMock.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "user@example.com", Password = "WrongPassword!" };
        var authResult = AuthResult.Success("jwt-token-value");

        _identityServiceMock
            .Setup(s => s.AuthenticateAsync(loginRequest.Email, loginRequest.Password))
            .ReturnsAsync(authResult);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseJson = JsonSerializer.Serialize(okResult.Value);
        var responseDoc = JsonDocument.Parse(responseJson);
        var root = responseDoc.RootElement;

        Assert.True(root.TryGetProperty("token", out var tokenProperty));
        Assert.Equal("jwt-token-value", tokenProperty.GetString());
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "user@example.com", Password = "WrongPassword!" };
        var errors = new[] { "Invalid credentials" };
        var authResult = AuthResult.Failure(errors);

        _identityServiceMock
            .Setup(s => s.AuthenticateAsync(loginRequest.Email, loginRequest.Password))
            .ReturnsAsync(authResult);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var responseJson = JsonSerializer.Serialize(unauthorizedResult.Value);
        var responseDoc = JsonDocument.Parse(responseJson);
        var root = responseDoc.RootElement;

        Assert.True(root.TryGetProperty("errors", out var errorsProperty));
        var errorsArray = errorsProperty.EnumerateArray().ToArray();
        Assert.Single(errorsArray);
        Assert.Equal("Invalid credentials", errorsArray[0].GetString());
    }


}