using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiniHubApi.Application.DTOs.External;
using MiniHubApi.Application.Services.Interfaces;
using MiniHubApi.Domain.Entities;
using MiniHubApi.Infrastructure.Data;

namespace MiniHubApi.Application.Services.Implementations;

public class DataImportService : IDataImportService
{
        private readonly ApplicationDbContext _context;
        private readonly IExternalApiService _externalApiService;
        private readonly ILogger<DataImportService> _logger;

        public DataImportService(
            ApplicationDbContext context,
            IExternalApiService externalApiService,
            ILogger<DataImportService> logger)
        {
            _context = context;
            _externalApiService = externalApiService;
            _logger = logger;
        }
        
  public async Task<ImportResult> ImportCategoriesAsync()
  {
    var resultado = new ImportResult { StartedAt = DateTime.UtcNow };
    
    try
    {
        _logger.LogInformation("Iniciando importação de categorias...");
        
        var categoriasExternas = await _externalApiService.GetCategoriesAsync();
        _logger.LogInformation($"Encontradas {categoriasExternas.Count} categorias na API");
        
        foreach (var categoriaExterna in categoriasExternas)
        {
            try
            {
                await ProcessarCategoriaAsync(categoriaExterna, resultado);
            }
            catch (Exception ex)
            {
                resultado.Failed++;
                resultado.Errors.Add($"Categoria {categoriaExterna.ExternalId}: {ex.Message}");
                _logger.LogWarning(ex, $"Erro na categoria {categoriaExterna.ExternalId}");
            }
        }
        
        await _context.SaveChangesAsync();
        
        resultado.Success = resultado.Failed == 0;
        resultado.Message = $"Importação de categorias: {resultado.Created} criadas, {resultado.Updated} atualizadas";
        
        _logger.LogInformation($"{resultado.Message}");
    }
    catch (Exception ex)
    {
        resultado.Success = false;
        resultado.Errors.Add($"Erro geral: {ex.Message}");
        _logger.LogError(ex, "Erro na importação de categorias");
    }
    finally
    {
        resultado.CompletedAt = DateTime.UtcNow;
    }
    
    return resultado;
  }
        
  private async Task ProcessarCategoriaAsync(ExternalCategoryDto categoriaExterna, ImportResult resultado)
  {
        var categoriaExistente = await _context.Categories
            .FirstOrDefaultAsync(c => c.ExternalId == categoriaExterna.ExternalId);
        
        if (categoriaExistente == null)
        {
            var novaCategoria = new Category
            {
                Name = categoriaExterna.Name,
                ExternalId = categoriaExterna.ExternalId
            };
            
            _context.Categories.Add(novaCategoria);
            resultado.Created++;
            
            _logger.LogInformation($"Categoria criada: {novaCategoria.Name}");
        }
        else
        {
            categoriaExistente.Name = categoriaExterna.Name;
            resultado.Updated++;
            
            _logger.LogInformation($"Categoria atualizada: {categoriaExistente.Name}");
        }
  }
        
  public async Task<ImportResult> ImportProductAsync() 
  {
        var resultado = new ImportResult { StartedAt = DateTime.UtcNow };

        try
        {
            _logger.LogInformation("Iniciando importação de produtos...");
            
            var produtosExternos = await _externalApiService.GetAllProductsAsync();
            _logger.LogInformation($"Encontrados {produtosExternos.Count} produtos na API");
            
            foreach (var produtoExterno in produtosExternos)
            {
                try
                {
                    await ProcessarProdutoAsync(produtoExterno, resultado);
                }
                catch (Exception ex)
                {
                    resultado.Failed++;
                    resultado.Errors.Add($"Produto {produtoExterno.ExternalId}: {ex.Message}");
                    _logger.LogWarning(ex, $"Erro no produto {produtoExterno.ExternalId}");
                }
            }
            
            await _context.SaveChangesAsync();
            
            resultado.Success = resultado.Failed == 0;
            resultado.Message = $"Importação de produtos: {resultado.Created} criados, {resultado.Updated} atualizados";
            
            _logger.LogInformation($"{resultado.Message}");
        }
        catch (Exception ex)
        {
            resultado.Success = false;
            resultado.Errors.Add($"Erro geral: {ex.Message}");
            _logger.LogError(ex, "Erro na importação de produtos");
        }
        finally
        {
            resultado.CompletedAt = DateTime.UtcNow;
        }

        return resultado;
  }
        
  private async Task ProcessarProdutoAsync(ExternalProductDto produtoExterno, ImportResult resultado)
{
    Category? categoria = null;
    if (!string.IsNullOrEmpty(produtoExterno.CategoryExternalId))
    {
        categoria = await _context.Categories
            .FirstOrDefaultAsync(c => c.ExternalId == produtoExterno.CategoryExternalId);
        
        if (categoria == null)
        {
            _logger.LogWarning($"Categoria {produtoExterno.CategoryExternalId} não encontrada. Criando temporária...");
            
            categoria = new Category
            {
                Name = $"Categoria {produtoExterno.CategoryExternalId}",
                ExternalId = produtoExterno.CategoryExternalId
            };
            
            _context.Categories.Add(categoria);
            await _context.SaveChangesAsync();
        }
    }
    
    var produtoExistente = await _context.Items
        .FirstOrDefaultAsync(p => p.ExternalId == produtoExterno.ExternalId);
    
    if (produtoExistente == null)
    {
        var novoProduto = new Item
        {
            Id = Guid.NewGuid(),
            ExternalId = produtoExterno.ExternalId,
            Nome = produtoExterno.Name,
            Descricao = produtoExterno.Description,
            Preco = produtoExterno.Price,
            Ativo = produtoExterno.Active,
            Estoque = produtoExterno.Stock,
            CategoryExternalId = produtoExterno.CategoryExternalId,
            CategoryId = categoria?.Id
        };
        await ProcessarTagsDoProdutoAsync(produtoExterno, novoProduto);
        _context.Items.Add(novoProduto);
        resultado.Created++;
        
        _logger.LogInformation($"Produto criado: {novoProduto.Nome} (Estoque: {novoProduto.Estoque})");
    }
    else
    {
        produtoExistente.Nome = produtoExterno.Name;
        produtoExistente.Descricao = produtoExterno.Description;
        produtoExistente.Preco = produtoExterno.Price;
        produtoExistente.Ativo = produtoExterno.Active;
        produtoExistente.Estoque = produtoExterno.Stock;
        produtoExistente.CategoryExternalId = produtoExterno.CategoryExternalId;
        produtoExistente.CategoryId = categoria?.Id;
        
        await ProcessarTagsDoProdutoAsync(produtoExterno, produtoExistente);
        resultado.Updated++;
        
        _logger.LogInformation($"↻ Produto atualizado: {produtoExistente.Nome}");
    }
  }

  public async Task ProcessarTagsDoProdutoAsync(ExternalProductDto produtoExterno, Item produto)
  {
    var nomesTags = produtoExterno.GetTags();
    
    if (!nomesTags.Any())
    {
        produto.Tags = new List<Tag>();
        return;
    }
    
    var nomesTagsNormalizados = nomesTags
        .Where(nome => !string.IsNullOrWhiteSpace(nome))
        .Select(nome => nome.Trim())
        .Distinct()
        .ToList();
    
    if (!nomesTagsNormalizados.Any())
    {
        produto.Tags = new List<Tag>();
        return;
    }
    
    var tagsExistentes = await _context.Tags
        .Where(t => nomesTagsNormalizados.Select(n => n.ToLower())
                    .Contains(t.Name.ToLower()))
        .ToListAsync();
    
    var tagsExistentesDict = tagsExistentes
        .ToDictionary(t => t.Name.ToLower(), t => t);
    
    var tagsParaProduto = new List<Tag>();
    var novasTags = new List<Tag>();
    
    foreach (var nomeTag in nomesTagsNormalizados)
    {
        if (tagsExistentesDict.TryGetValue(nomeTag.ToLower(), out var tagExistente))
        {
            tagsParaProduto.Add(tagExistente);
        }
        else
        {
            var novaTag = new Tag
            {
                Id = Guid.NewGuid(),
                Name = nomeTag
            };
            
            novasTags.Add(novaTag);
            tagsParaProduto.Add(novaTag);
            
            _logger.LogDebug($"Nova tag criada: {nomeTag}");
        }
    }
    if (novasTags.Any())
    {
        await _context.Tags.AddRangeAsync(novasTags);
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"{novasTags.Count} novas tags criadas");
    }
    
    produto.Tags = tagsParaProduto;
  }
}