using System.Text.Json.Serialization;

namespace MiniHubApi.Application.DTOs.Responses;

public class PagedResponse<T>
{
    public List<T> Data { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public PagedResponse(
        List<T> data, 
        int page, 
        int pageSize, 
        int totalCount)
    {
        Data = data;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}