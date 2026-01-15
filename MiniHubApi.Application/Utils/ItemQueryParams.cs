using System.ComponentModel.DataAnnotations;

namespace MiniHubApi.Application.Utils;

public class ItemQueryParams
{
    public string? Search { get; set; }
    public string OrderBy { get; set; } = "nome";
    public string OrderDirection { get; set; } = "ASC";
    
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
    
    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}