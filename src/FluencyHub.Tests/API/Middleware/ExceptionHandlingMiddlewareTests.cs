using System.Net;
using System.Text.Json;
using FluencyHub.API.Middleware;
using FluencyHub.Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using FluentValidation.Results;

namespace FluencyHub.Tests.API.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;
    private readonly ExceptionHandlingMiddleware _middleware;
    private readonly HttpContext _httpContext;
    private readonly TestableRequestDelegate _requestDelegate;
    
    public ExceptionHandlingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        _requestDelegate = new TestableRequestDelegate();
        _middleware = new ExceptionHandlingMiddleware(_requestDelegate.Invoke, _loggerMock.Object);
        
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }
    
    [Fact]
    public async Task InvokeAsync_NoException_PassesThrough()
    {
        // Arrange
        _requestDelegate.SetSuccess();
        
        // Act
        await _middleware.InvokeAsync(_httpContext);
        
        // Assert
        Assert.Equal(200, _httpContext.Response.StatusCode);
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);
    }
    
    [Fact]
    public async Task InvokeAsync_ValidationException_Returns400BadRequest()
    {
        // Arrange
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "O nome é obrigatório")
        };
        var validationException = new ValidationException(validationFailures);
        _requestDelegate.SetException(validationException);
        
        // Act
        await _middleware.InvokeAsync(_httpContext);
        
        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, _httpContext.Response.StatusCode);
        
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.Equal("Requisição Inválida", response.GetProperty("title").GetString());
        Assert.Equal(400, response.GetProperty("status").GetInt32());
    }
    
    [Fact]
    public async Task InvokeAsync_NotFoundException_Returns404NotFound()
    {
        // Arrange
        var notFoundException = new NotFoundException("Entity", "123");
        _requestDelegate.SetException(notFoundException);
        
        // Act
        await _middleware.InvokeAsync(_httpContext);
        
        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, _httpContext.Response.StatusCode);
        
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.Equal("Erro no Servidor", response.GetProperty("title").GetString());
        Assert.Equal(404, response.GetProperty("status").GetInt32());
        Assert.Contains("Entidade não encontrada", response.GetProperty("detail").GetString());
    }
    
    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_Returns401Unauthorized()
    {
        // Arrange
        var unauthorizedException = new UnauthorizedAccessException("Acesso não autorizado");
        _requestDelegate.SetException(unauthorizedException);
        
        // Act
        await _middleware.InvokeAsync(_httpContext);
        
        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, _httpContext.Response.StatusCode);
        
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.Equal("Erro no Servidor", response.GetProperty("title").GetString());
        Assert.Equal(401, response.GetProperty("status").GetInt32());
        Assert.Equal("Acesso não autorizado", response.GetProperty("detail").GetString());
    }
    
    [Fact]
    public async Task InvokeAsync_GenericException_Returns500InternalServerError()
    {
        // Arrange
        var genericException = new Exception("Um erro inesperado ocorreu");
        _requestDelegate.SetException(genericException);
        
        // Act
        await _middleware.InvokeAsync(_httpContext);
        
        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, _httpContext.Response.StatusCode);
        
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.Equal("Erro no Servidor", response.GetProperty("title").GetString());
        Assert.Equal(500, response.GetProperty("status").GetInt32());
        Assert.Equal("Um erro inesperado ocorreu", response.GetProperty("detail").GetString());
    }
    
    [Fact]
    public async Task InvokeAsync_ApplicationException_ReturnsMessageAsTitle()
    {
        // Arrange
        var applicationException = new ApplicationException("Custom application error");
        _requestDelegate.SetException(applicationException);
        
        // Act
        await _middleware.InvokeAsync(_httpContext);
        
        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, _httpContext.Response.StatusCode);
        
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.Equal("Custom application error", response.GetProperty("title").GetString());
        Assert.Equal(500, response.GetProperty("status").GetInt32());
    }
    
    public class TestableRequestDelegate
    {
        private RequestDelegate _requestDelegate;
        
        public void SetSuccess()
        {
            _requestDelegate = _ => Task.CompletedTask;
        }
        
        public void SetException(Exception exception)
        {
            _requestDelegate = _ => throw exception;
        }
        
        public Task Invoke(HttpContext context)
        {
            return _requestDelegate(context);
        }
    }
} 