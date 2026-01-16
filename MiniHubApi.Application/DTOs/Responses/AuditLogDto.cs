namespace MiniHubApi.Application.DTOs.Responses;

public class AuditLogDto
{
    public string Id { get; set; }
    public string Action { get; set; }
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Details { get; set; }
    public string UserId { get; set; }
}