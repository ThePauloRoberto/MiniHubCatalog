using MiniHubApi.Application.DTOs.External;

namespace MiniHubApi.Application.Services.Interfaces;

public interface IExternalApiService
{
    Task<List<ExternalProductDto>> GetAllProductsAsync();
    
    Task<List<ExternalCategoryDto>> GetCategoriesAsync();
}