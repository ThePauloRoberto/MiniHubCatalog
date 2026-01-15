using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MiniHubApi.Domain.Audit;

public class ErroLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
        
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }
        
    [BsonElement("level")]
    public string Level { get; set; } // "ERROR", "WARNING", "INFO"
        
    [BsonElement("message")]
    public string Message { get; set; }
        
    [BsonElement("exception")]
    public string Exception { get; set; }
        
    [BsonElement("stackTrace")]
    public string StackTrace { get; set; }
        
    [BsonElement("service")]
    public string Service { get; set; } // "ItemService", "DataImportService"
        
    [BsonElement("method")]
    public string Method { get; set; }
        
    [BsonElement("correlationId")]
    public string CorrelationId { get; set; }
        
    [BsonElement("userId")]
    public string UserId { get; set; }
        
    [BsonElement("requestPath")]
    public string RequestPath { get; set; }
}