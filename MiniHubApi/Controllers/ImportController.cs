using Microsoft.AspNetCore.Mvc;
using MiniHubApi.Application.Services.Interfaces;

namespace MiniHubApi.Controllers;

[ApiController]
[Route("api/[controller]")]  // Vira: /api/import
public class ImportController : ControllerBase
{
    private readonly IExternalApiService _externalApiService;
    private readonly ILogger<ImportController> _logger;
    private readonly IDataImportService _importService;

    // O ASP.NET INJETA automaticamente o serviço aqui!
    public ImportController(
        IExternalApiService externalApiService,
        ILogger<ImportController> logger,
        IDataImportService importService)
    {
        _externalApiService = externalApiService;
        _logger = logger;
        _importService = importService;
    }

    [HttpPost("categories")]
    public async Task<ActionResult> ImportCategories()
    {
        _logger.LogInformation("Solicitada importação de categorias...");
        
        var resultado = await _importService.ImportCategoriesAsync();
    
        return Ok(new
        {
            success = resultado.Success,
            message = resultado.Message,
            stats = new
            {
                created = resultado.Created,
                updated = resultado.Updated,
                failed = resultado.Failed
            },
            errors = resultado.Errors
        });
    }
    
    
    [HttpPost("products")]
    public async Task<ActionResult> ImportProducts()
    {
        _logger.LogInformation("Solicitada importação de produtos...");
    
        var resultado = await _importService.ImportProductAsync();
    
        return Ok(new
        {
            success = resultado.Success,
            message = resultado.Message,
            stats = new
            {
                created = resultado.Created,
                updated = resultado.Updated,
                failed = resultado.Failed
            },
            errors = resultado.Errors.Take(10)  // Mostra só 10 erros
        });
    }
    
    
    
}