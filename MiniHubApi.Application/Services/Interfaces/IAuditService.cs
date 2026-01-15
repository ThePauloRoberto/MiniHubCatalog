using MiniHubApi.Application.DTOs.Responses;

namespace MiniHubApi.Application.Services.Interfaces;

public interface IAuditService
{
    Task LogEntityActionAsync(string action, string entityType, string entityId, 
        object oldValues = null, object newValues = null);
    
    Task<string> StartImportLogAsync(string importType, string initiatedBy = null);
    Task CompleteImportLogAsync(string importLogId, ImportResult result);
    Task FailImportLogAsync(string importLogId, Exception exception, List<string> errors = null);
    
    Task LogErrorAsync(Exception exception, string service = null, string method = null);
    
    Task<PagedResponse<AuditLogDto>> GetAuditLogsAsync(
        string entityType = null,
        string action = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 10);
            
    Task<PagedResponse<ImportLogDto>> GetImportLogsAsync(
        string importType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 10);
}
}