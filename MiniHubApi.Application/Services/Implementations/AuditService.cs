using Microsoft.Extensions.Logging;
using MiniHubApi.Application.Services.Interfaces;
using MiniHubApi.Domain.Audit;
using MongoDB.Driver;

namespace MiniHubApi.Application.Services.Implementations;

public class AuditService : IAuditService
{
        private readonly IMongoCollection<AuditLog> _auditLogs;
        private readonly ILogger<AuditService> _logger;

        public AuditService(IMongoDatabase mongoDatabase, ILogger<AuditService> logger)
        {
            _auditLogs = mongoDatabase.GetCollection<AuditLog>("AuditLogs");
            _logger = logger;
        }

        public async Task LogActionAsync(string action, string entityType, string entityId, object newValues,
            string details = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    Details = details,
                    Timestamp = DateTime.UtcNow,
                    UserId = "API"
                };

                await _auditLogs.InsertOneAsync(auditLog);
                
                _logger.LogDebug("Audit logged: {Action} {EntityType} {EntityId}", 
                    action, entityType, entityId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to audit {Action} for {EntityType} {EntityId}", 
                    action, entityType, entityId);
                // Não lança exceção para não quebrar o fluxo principal
            }
        }

        public async Task<List<AuditLog>> GetLogsAsync(
            string entityType = null, 
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
}