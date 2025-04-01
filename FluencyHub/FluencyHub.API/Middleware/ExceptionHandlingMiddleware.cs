using System.Net;
using System.Text.Json;
using FluencyHub.Application.Common.Exceptions;

namespace FluencyHub.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        
        var response = new
        {
            title = GetTitle(exception),
            status = statusCode,
            detail = GetDetail(exception),
            errors = GetErrors(exception)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static int GetStatusCode(Exception exception) =>
        exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
        
    private static string GetTitle(Exception exception) =>
        exception switch
        {
            ValidationException => "Bad Request",
            ApplicationException appEx => appEx.Message,
            _ => "Server Error"
        };
        
    private static string GetDetail(Exception exception) =>
        exception switch
        {
            NotFoundException notFoundEx => $"Entity not found: {notFoundEx.Message}",
            _ => exception.Message
        };
        
    private static IReadOnlyDictionary<string, string[]>? GetErrors(Exception exception)
    {
        if (exception is ValidationException validationException)
        {
            return validationException.Errors;
        }
        
        return null;
    }
} 