using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MiniHubApi.Application.Services.Interfaces;
using MiniHubApi.Domain.Audit;
using MongoDB.Driver;
using System.Security.Claims;
using System.Text.Json;

namespace MiniHubApi.Application.Services.Implementations;

public class AuditService : IAuditService
{
    private readonly IMongoCollection<AuditLog> _auditLogs;
    private readonly ILogger<AuditService> _logger;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AuditService(
        IMongoDatabase mongoDatabase, 
        ILogger<AuditService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _auditLogs = mongoDatabase.GetCollection<AuditLog>("AuditLogs");
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogActionAsync(string action, string entityType, string entityId, object newValues, string details = null)
    {
        try
        {
            var resolvedUserId = await GetCurrentUserIdAsync(entityId);

            var auditLog = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details ?? GetDefaultDetails(action, entityType, entityId),
                Timestamp = DateTime.UtcNow,
                UserId = resolvedUserId,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent(),
                Payload = newValues != null ? 
                    JsonSerializer.Serialize(newValues) : null
            };

            await _auditLogs.InsertOneAsync(auditLog);
            
            _logger.LogDebug("Audit logged: {Action} {EntityType} {EntityId} by {UserId}", 
                action, entityType, entityId, resolvedUserId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to audit {Action} for {EntityType} {EntityId}", 
                action, entityType, entityId);
            // Não lança exceção para não quebrar o fluxo principal
        }
    }
    

    public async Task<List<AuditLog>> GetLogsAsync(
        string? entityType = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        int limit = 100)
    {
        try
        {
            var filter = Builders<AuditLog>.Filter.Empty;
            
            if (!string.IsNullOrEmpty(entityType))
                filter &= Builders<AuditLog>.Filter.Eq(a => a.EntityType, entityType);
                
            if (startDate.HasValue)
                filter &= Builders<AuditLog>.Filter.Gte(a => a.Timestamp, startDate.Value);
                
            if (endDate.HasValue)
                filter &= Builders<AuditLog>.Filter.Lte(a => a.Timestamp, endDate.Value);
            
            return await _auditLogs.Find(filter)
                .SortByDescending(a => a.Timestamp)
                .Limit(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs");
            return new List<AuditLog>();
        }
    }

    public async Task LogImportAsync(string source, int itemsCount, bool success, string? errorMessage = null)
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            
            var auditLog = new AuditLog
            {
                Action = "Import",
                EntityType = "Batch",
                EntityId = Guid.NewGuid().ToString(),
                Details = $"Import from {source}: {itemsCount} items. Success: {success}",
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                IpAddress = GetClientIpAddress(),
                Payload = errorMessage != null ? $"Error: {errorMessage}" : null
            };

            await _auditLogs.InsertOneAsync(auditLog);
            
            _logger.LogInformation("Import logged: {Source} {ItemsCount} by {UserId}", 
                source, itemsCount, userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log import from {Source}", source);
        }
    }

    public async Task LogExportAsync(string reportType, string fileName, bool success)
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            
            var auditLog = new AuditLog
            {
                Action = "Export",
                EntityType = "Report",
                EntityId = Guid.NewGuid().ToString(),
                Details = $"Export {reportType} as {fileName}. Success: {success}",
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                IpAddress = GetClientIpAddress(),
                Payload = null
            };

            await _auditLogs.InsertOneAsync(auditLog);
            
            _logger.LogInformation("Export logged: {ReportType} by {UserId}", 
                reportType, userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log export {ReportType}", reportType);
        }
    }

    public async Task LogErrorAsync(string methodName, Exception exception, object? parameters = null)
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            
            var payload = new
            {
                parameters,
                stackTrace = exception.StackTrace
            };
            
            var auditLog = new AuditLog
            {
                Action = "Error",
                EntityType = "System",
                EntityId = Guid.NewGuid().ToString(),
                Details = $"Error in {methodName}: {exception.Message}",
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                IpAddress = GetClientIpAddress(),
                Payload = JsonSerializer.Serialize(payload)
            };

            await _auditLogs.InsertOneAsync(auditLog);
            
            _logger.LogError(exception, "Error logged in {MethodName} by {UserId}", 
                methodName, userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log error in {MethodName}", methodName);
        }
    }
    

    private async Task<string> GetCurrentUserIdAsync(string? providedUserId = null)
    {
        if (!string.IsNullOrEmpty(providedUserId))
            return providedUserId;
        
        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                       ?? httpContext.User.FindFirst("sub")?.Value;
            
            if (!string.IsNullOrEmpty(userId))
                return userId;
        }
        
        return "System";
    }

    private string? GetClientIpAddress()
    {
        try
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            if (httpContext == null) return null;
            
            if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedHeader))
            {
                return forwardedHeader.ToString().Split(',')[0].Trim();
            }

            return httpContext.Connection?.RemoteIpAddress?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private string? GetUserAgent()
    {
        try
        {
            return _httpContextAccessor?.HttpContext?.Request.Headers["User-Agent"].ToString();
        }
        catch
        {
            return null;
        }
    }

    private static string GetDefaultDetails(string action, string entityType, string entityId)
    {
        return $"{action} performed on {entityType} with ID: {entityId}";
    }
    
}