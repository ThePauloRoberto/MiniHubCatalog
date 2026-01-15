using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MiniHubApi.Middlewares;

    public class ServiceLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ServiceLoggingMiddleware> _logger;

        public ServiceLoggingMiddleware(RequestDelegate next, ILogger<ServiceLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var controller = context.GetRouteValue("controller")?.ToString() ?? "Unknown";
            var action = context.GetRouteValue("action")?.ToString() ?? "Unknown";
            var serviceName = $"{controller}Service";
            
            _logger.LogInformation("Service method called: {Service}.{Action}", serviceName, action);
            
            var startTime = DateTime.UtcNow;
            
            await _next(context);
            
            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogInformation("Service method completed: {Service}.{Action} in {ElapsedMs}ms", 
                serviceName, action, elapsed.TotalMilliseconds);
        }
    }