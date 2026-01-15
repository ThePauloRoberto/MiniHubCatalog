using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MiniHubApi.Middlewares;

public class ServiceExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ServiceExceptionMiddleware> _logger;

    public ServiceExceptionMiddleware(RequestDelegate next, ILogger<ServiceExceptionMiddleware> logger)
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
            _logger.LogError(ex, "Service error: {Message}", ex.Message);
            
            var (statusCode, message) = GetErrorResponse(ex);
                
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
                
            var response = new
            {
                StatusCode = statusCode,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
                
            await context.Response.WriteAsJsonAsync(response);
        }
    }

    private (int statusCode, string message) GetErrorResponse(Exception ex)
    {
        return ex switch
        {
            ArgumentException => (400, ex.Message),
            InvalidOperationException => (400, ex.Message),
            KeyNotFoundException => (404, ex.Message),
            UnauthorizedAccessException => (401, ex.Message),
            DbUpdateException dbEx when dbEx.InnerException?.Message?.Contains("Duplicate") == true 
                => (409, "Duplicate entry"),
            DbUpdateException => (400, "Database error"),
            _ => (500, "Internal server error")
        };
    }
}
