using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MiniHubApi.Domain.Audit;

public class AuditLog
{  [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
        
    [BsonElement("action")]
    public string Action { get; set; } // "CREATE", "UPDATE", "DELETE", "IMPORT"
        
    [BsonElement("entityType")]
    public string EntityType { get; set; } // "Item", "Category", "Tag"
        
    [BsonElement("entityId")]
    public string EntityId { get; set; }
        
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
    [BsonElement("details")]
    public string Details { get; set; }
        
    [BsonElement("userId")]
    public string UserId { get; set; } = "System";
}