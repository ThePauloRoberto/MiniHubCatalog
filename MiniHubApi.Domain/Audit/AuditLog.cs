using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MiniHubApi.Domain.Audit;

public class AuditLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
        
    [BsonElement("userId")]
    public string UserId { get; set; }
        
    [BsonElement("entityType")]
    public string EntityType { get; set; } // "Item", "Category", "Tag"
        
    [BsonElement("entityId")]
    public string EntityId { get; set; }
        
    [BsonElement("action")]
    public string Action { get; set; } // "CREATE", "UPDATE", "DELETE", "IMPORT"
        
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }
        
    [BsonElement("oldValues")]
    public BsonDocument OldValues { get; set; }
        
    [BsonElement("newValues")]
    public BsonDocument NewValues { get; set; }
        
    [BsonElement("ipAddress")]
    public string IpAddress { get; set; }
        
    [BsonElement("userAgent")]
    public string UserAgent { get; set; }
        
    [BsonElement("correlationId")]
    public string CorrelationId { get; set; }
}