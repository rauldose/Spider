using System.Text.Json;
using Spider.ConnectionManagement.Domain.Exceptions;

namespace Spider.ConnectionManagement.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An error occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ConnectionNotFoundException ex => new ErrorResponse
            {
                StatusCode = 404,
                Message = ex.Message,
                Type = "NotFound"
            },
            InvalidConnectionParametersException ex => new ErrorResponse
            {
                StatusCode = 400,
                Message = ex.Message,
                Type = "ValidationError"
            },
            ConnectionException ex => new ErrorResponse
            {
                StatusCode = 400,
                Message = ex.Message,
                Type = "ConnectionError"
            },
            ArgumentException ex => new ErrorResponse
            {
                StatusCode = 400,
                Message = ex.Message,
                Type = "ValidationError"
            },
            _ => new ErrorResponse
            {
                StatusCode = 500,
                Message = "An unexpected error occurred",
                Type = "InternalServerError"
            }
        };

        context.Response.StatusCode = response.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}