using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MiniHubApi.Domain.Audit;

public class ImportLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
        
    [BsonElement("importId")]
    public string ImportId { get; set; }
        
    [BsonElement("importType")]
    public string ImportType { get; set; } // "PRODUCTS", "CATEGORIES"
        
    [BsonElement("startTime")]
    public DateTime StartTime { get; set; }
        
    [BsonElement("endTime")]
    public DateTime? EndTime { get; set; }
        
    [BsonElement("status")]
    public string Status { get; set; } // "RUNNING", "COMPLETED", "FAILED"
        
    [BsonElement("itemsProcessed")]
    public int ItemsProcessed { get; set; }
        
    [BsonElement("created")]
    public int Created { get; set; }
        
    [BsonElement("updated")]
    public int Updated { get; set; }
        
    [BsonElement("failed")]
    public int Failed { get; set; }
        
    [BsonElement("errorMessages")]
    public List<string> ErrorMessages { get; set; } = new();
        
    [BsonElement("durationInSeconds")]
    public double DurationInSeconds { get; set; }
        
    [BsonElement("initiatedBy")]
    public string InitiatedBy { get; set; }
}