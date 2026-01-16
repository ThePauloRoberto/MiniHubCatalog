using MiniHubApi.Application.DTOs.Responses;
using MiniHubApi.Domain.Audit;

namespace MiniHubApi.Application.Services.Interfaces;

public interface IAuditService
{
    Task LogActionAsync(string action, string entityType, string entityId, object newValues, string details = null);
    
    Task<List<AuditLog>> GetLogsAsync(
        string entityType = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        int limit = 100);
}
