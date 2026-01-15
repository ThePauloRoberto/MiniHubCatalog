namespace MiniHubApi.Application.Configuration;

public class MongoDBSettings
{
    public const string SectionName = "MongoDBSettings";
        
    public string DatabaseName { get; set; } = "MiniHubAuditoria";
    public string AuditCollection { get; set; } = "AuditLogs";
    public string ImportLogsCollection { get; set; } = "ImportLogs";
    public string ErrorLogsCollection { get; set; } = "ErrorLogs";
}