using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniHubApi.Application.DTOs;
using MiniHubApi.Application.DTOs.External;
using MiniHubApi.Domain.Entities;
using MiniHubApi.Infrastructure.Data;

namespace MiniHubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemController : ControllerBase
{
  private readonly ApplicationDbContext _context;
  private readonly ILogger<ItemController> _logger;

  public ItemController(ApplicationDbContext context, ILogger<ItemController> logger)
  {
    _context = context;
    _logger = logger;
  }
  
  [HttpGet]
  public async Task<ActionResult<IEnumerable<ItemDto>>> GetItems()
  {
      try
      {
          var items = await _context.Items
              .Include(i => i.Categoria)
              .Include(i => i.Tags)
              .Where(i => i.Ativo)
              .Select(i => new ItemDto
              {
                  Id = i.Id,
                  Nome = i.Nome,
                  Descricao = i.Descricao,
                  Preco = i.Preco,
                  Ativo = i.Ativo,
                  Estoque = i.Estoque,
                  ExternalId = i.ExternalId,
                  CategoryExternalId = i.CategoryExternalId,
                  CategoryId = i.CategoryId,
                  Categoria = i.Categoria != null ? new CategoryDto
                  {
                      Id = i.Categoria.Id,
                      Name = i.Categoria.Name,
                      ExternalId = i.Categoria.ExternalId,
                      ItemCount = 0
                  } : null,
                  Tags = i.Tags.Select(t => new TagDto
                  {
                      Id = t.Id,
                      Name = t.Name,
                      ItemCount = 0
                  }).ToList(),
              })
              .ToListAsync();
        
          return Ok(items);
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Erro ao buscar itens");
          return StatusCode(500, new { message = "Erro interno do servidor" });
      }
  }
  
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Item>> GetItem(Guid id)
    {
        try
        {
            var item = await _context.Items
                .Include(i => i.Categoria)
                .Include(i => i.Tags)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound(new { message = $"Item com ID {id} não encontrado" });
            }

            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao buscar item {id}");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
    
    [HttpPost]
    public async Task<ActionResult<Item>> CreateItem([FromBody] CreateItemDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                return BadRequest(new { message = "Nome é obrigatório" });
            
            if (dto.Preco <= 0)
                return BadRequest(new { message = "Preço deve ser maior que zero" });
            
            if (dto.CategoryId.HasValue)
            {
                var categoryExists = await _context.Categories
                    .AnyAsync(c => c.Id == dto.CategoryId.Value);
                
                if (!categoryExists)
                    return BadRequest(new { message = "Categoria não encontrada" });
            }
            
            var item = new Item
            {
                Id = Guid.NewGuid(),
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Preco = dto.Preco,
                Ativo = dto.Ativo,
                Estoque = dto.Estoque,
                CategoryId = dto.CategoryId,
            };
            
            if (dto.TagIds != null && dto.TagIds.Any())
            {
                item.Tags = await _context.Tags
                    .Where(t => dto.TagIds.Contains(t.Id))
                    .ToListAsync();
            }

            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            
            var createdItem = await _context.Items
                .Include(i => i.Categoria)
                .Include(i => i.Tags)
                .FirstOrDefaultAsync(i => i.Id == item.Id);

            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, createdItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar item");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody]UpdateItemDto dto)
    {
        try 
        {
            var item = await _context.Items
                .Include(i => i.Tags)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound(new { message = $"Item com ID {id} não encontrado" });
            }
            
            if (!string.IsNullOrWhiteSpace(dto.Nome))
                item.Nome = dto.Nome;
                
            if (!string.IsNullOrWhiteSpace(dto.Descricao))
                item.Descricao = dto.Descricao;
                
            if (dto.Preco.HasValue)
            {
                if (dto.Preco.Value <= 0)
                    return BadRequest(new { message = "Preço deve ser maior que zero" });
                item.Preco = dto.Preco.Value;
            }
            
            if (dto.Ativo.HasValue)
                item.Ativo = dto.Ativo.Value;
                
            if (dto.Estoque.HasValue)
                item.Estoque = dto.Estoque.Value;

            // Se CategoryId foi enviado, verifica se existe
            if (dto.CategoryId.HasValue)
            {
                if (dto.CategoryId.Value > 0)
                {
                    var categoryExists = await _context.Categories
                        .AnyAsync(c => c.Id == dto.CategoryId.Value);
                        
                    if (!categoryExists)
                    {
                        return BadRequest(new { message = $"Categoria com ID {dto.CategoryId} não encontrada" });
                    }
                    item.CategoryId = dto.CategoryId.Value;
                }
                else if (dto.CategoryId.Value == 0)
                {
                    // Se enviou 0, remove a categoria
                    item.CategoryId = null;
                }
            }

            // Atualiza tags se foram enviadas
            if (dto.TagIds != null)
            {
                var tags = await _context.Tags
                    .Where(t => dto.TagIds.Contains(t.Id))
                    .ToListAsync();
                item.Tags = tags;
            }
            
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao atualizar item {id}");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteItem(Guid id)
    {
        try
        {
            var item = await _context.Items.FindAsync(id);
            
            if (item == null)
            {
                return NotFound(new { message = $"Item com ID {id} não encontrado" });
            }
            
            item.Ativo = false;
            
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao deletar item {id}");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
    
    private async Task<bool> ItemExistsAsync(Guid id)
    {
        return await _context.Items.AnyAsync(e => e.Id == id);
    }

    [HttpPost("import")]
    public async Task<ActionResult> ImportProduct([FromBody] ImportProductDto dto)
    {

        try
        {
            if (string.IsNullOrWhiteSpace(dto.ExternalId))
                return BadRequest(new { message = "ExternalId é obrigatório para importação" });
            
            var existingItem = await _context.Items
                .FirstOrDefaultAsync(i => i.ExternalId == dto.ExternalId);

            if (existingItem != null)
                return Conflict(new { message = $"Item com ExternalId {dto.ExternalId} já existe" });
            
            Category? category = null;
            if (!string.IsNullOrEmpty(dto.CategoryExternalId))
            {
                category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.ExternalId == dto.CategoryExternalId);
            }
            
            var item = new Item
            {
                Id = Guid.NewGuid(),
                ExternalId = dto.ExternalId,
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Preco = dto.Preco,
                Ativo = dto.Ativo,
                Estoque = dto.Estoque,
                CategoryExternalId = dto.CategoryExternalId,
                CategoryId = category?.Id,
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao importar item {dto.ExternalId}");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}