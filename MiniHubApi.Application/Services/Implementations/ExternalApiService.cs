using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniHubApi.Application.Configuration;
using MiniHubApi.Application.DTOs.External;
using MiniHubApi.Application.Services.Interfaces;

namespace MiniHubApi.Application.Services.Implementations;

public class ExternalApiService : IExternalApiService
{
     private readonly HttpClient _httpClient;
        private readonly ExternalApiSettings _settings;
        private readonly ILogger<ExternalApiService> _logger;
        
        public ExternalApiService(
            HttpClient httpClient,
            IOptions<ExternalApiSettings> options,
            ILogger<ExternalApiService> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _logger = logger;
            
            ConfigurarHttpClient();
        }

        private void ConfigurarHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        }
        
        public async Task<List<ExternalCategoryDto>> GetCategoriesAsync()
        {
            try
            {
                var resposta = await _httpClient.GetAsync(_settings.CategoriesEndpoint);
                resposta.EnsureSuccessStatusCode();
                
                var json = await resposta.Content.ReadAsStringAsync();
                
                var categorias = JsonSerializer.Deserialize<List<ExternalCategoryDto>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                return categorias ?? new List<ExternalCategoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar categorias");
                throw;
            }
        }
        
        public async Task<List<ExternalProductDto>> GetAllProductsAsync()
        { 
            try 
            {
                var allProducts = new List<ExternalProductDto>();
                
                var categories = await GetCategoriesAsync();
                _logger.LogInformation($"Encontradas {categories.Count} categorias");
            
                if (categories.Count == 0)
                {
                    _logger.LogWarning("Nenhuma categoria encontrada!");
                    return allProducts;
                }
                
                foreach (var category in categories)
                {
                    try
                    {
                        var endpoint = $"Category/{category.ExternalId}/Product";
                        _logger.LogInformation($"Buscando: {endpoint}");
                        
                        var response = await _httpClient.GetAsync(endpoint);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var products = JsonSerializer.Deserialize<List<ExternalProductDto>>(json,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            
                            if (products != null && products.Count > 0)
                            {
                                allProducts.AddRange(products);
                                _logger.LogInformation($"Categoria {category.ExternalId}: {products.Count} produtos");
                            }
                            else
                            {
                                _logger.LogWarning($"Categoria {category.ExternalId}: Nenhum produto encontrado");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Categoria {category.ExternalId}: Erro {response.StatusCode}");
                        }
                        
                        await Task.Delay(100);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Erro na categoria {category.ExternalId}");
                    }
                }
            
                _logger.LogInformation($"Total de produtos: {allProducts.Count}");
                return allProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todos os produtos");
                return new List<ExternalProductDto>();
            }
        }
}
