using MiniHubApi.Application.DTOs;
using MiniHubApi.Application.DTOs.External;
using MiniHubApi.Application.DTOs.Responses;

namespace MiniHubApi.Application.Services.Interfaces
{
    public interface IItemService
    {
        Task<PagedResponse<ItemDto>> GetItemsAsync(
            string? searchTerm = null,
            string orderBy = "nome",
            string orderDirection = "ASC",
            int page = 1,
            int pageSize = 10);
        
        Task<ItemDto?> GetItemByIdAsync(Guid id);
        Task<ItemDto> CreateItemAsync(CreateItemDto createDto);
        Task<ItemDto?> UpdateItemAsync(Guid id, UpdateItemDto updateDto);
        Task<bool> DeleteItemAsync(Guid id);
        Task<ItemDto> ImportItemAsync(ImportProductDto importDto);
    }
}