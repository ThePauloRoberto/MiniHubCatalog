using MiniHubApi.Application.DTOs;
using MiniHubApi.Application.DTOs.Responses;

namespace MiniHubApi.Application.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<PagedResponse<CategoryDto>> GetCategoriesAsync(
            string? nameCategory = null,
            string orderBy = "name",
            string orderDirection = "ASC",
            int page = 1,
            int pageSize = 10);
        
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto);
        Task<CategoryDto?> UpdateCategoryAsync(int id, CreateCategoryDto updateDto);
        Task<bool> DeleteCategoryAsync(int id);
    }
}