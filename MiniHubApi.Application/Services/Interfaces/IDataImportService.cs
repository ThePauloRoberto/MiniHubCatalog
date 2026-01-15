namespace MiniHubApi.Application.Services.Interfaces;

public interface IDataImportService
{
    Task<ImportResult> ImportCategoriesAsync();
    Task<ImportResult> ImportProductAsync();
}

public class ImportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
}