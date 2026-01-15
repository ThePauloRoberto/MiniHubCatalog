using MiniHubApi.Application.DTOs;
using MiniHubApi.Application.DTOs.Responses;

namespace MiniHubApi.Application.Services.Interfaces;

public interface ITagService
{
        Task<PagedResponse<TagDto>> GetTagsAsync(
            string? nameTag = null,
            string orderBy = "name",
            string orderDirection = "ASC",
            int page = 1,
            int pageSize = 10);
        
        
        Task<TagDto> CreateTagAsync(CreateTagDto createDto);
}