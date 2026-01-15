namespace MiniHubApi.Application.DTOs.Responses;

public class ImportLogDto
{
    public string Id { get; set; }
    public string ImportType { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; }
    public int ItemsProcessed { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Failed { get; set; }
    public double DurationInSeconds { get; set; }
}