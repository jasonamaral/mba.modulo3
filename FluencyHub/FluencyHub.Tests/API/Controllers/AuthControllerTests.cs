using FluencyHub.API.Controllers;
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
        var loginRequest = new LoginRequest("user@example.com", "Password123!");
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
        var loginRequest = new LoginRequest("user@example.com", "WrongPassword!");
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
    
    [Fact]
    public async Task Register_WithValidData_ReturnsOkWithToken()
    {
        // Arrange
        var registerRequest = new RegisterRequest(
            "newuser@example.com", 
            "Password123!", 
            "John", 
            "Doe"
        );
        
        var authResult = AuthResult.Success("new-user-jwt-token");
        
        _identityServiceMock
            .Setup(s => s.RegisterUserAsync(
                registerRequest.Email,
                registerRequest.Password,
                registerRequest.FirstName,
                registerRequest.LastName))
            .ReturnsAsync(authResult);
        
        // Act
        var result = await _controller.Register(registerRequest);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseJson = JsonSerializer.Serialize(okResult.Value);
        var responseDoc = JsonDocument.Parse(responseJson);
        var root = responseDoc.RootElement;
        
        Assert.True(root.TryGetProperty("token", out var tokenProperty));
        Assert.Equal("new-user-jwt-token", tokenProperty.GetString());
    }
    
    [Fact]
    public async Task Register_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterRequest(
            "existing@example.com", 
            "Password123!", 
            "Jane", 
            "Smith"
        );
        
        var errors = new[] { "Email already exists" };
        var authResult = AuthResult.Failure(errors);
        
        _identityServiceMock
            .Setup(s => s.RegisterUserAsync(
                registerRequest.Email,
                registerRequest.Password,
                registerRequest.FirstName,
                registerRequest.LastName))
            .ReturnsAsync(authResult);
        
        // Act
        var result = await _controller.Register(registerRequest);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var responseJson = JsonSerializer.Serialize(badRequestResult.Value);
        var responseDoc = JsonDocument.Parse(responseJson);
        var root = responseDoc.RootElement;
        
        Assert.True(root.TryGetProperty("errors", out var errorsProperty));
        var errorsArray = errorsProperty.EnumerateArray().ToArray();
        Assert.Single(errorsArray);
        Assert.Equal("Email already exists", errorsArray[0].GetString());
    }
} 