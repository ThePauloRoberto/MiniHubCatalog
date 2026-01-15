using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniHubApi.Application.DTOs;
using MiniHubApi.Domain.Entities;
using MiniHubApi.Infrastructure.Data;

namespace MiniHubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
     private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            ApplicationDbContext context,
            ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories
        (
            [FromQuery] string? nameCategory,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string orderBy = "name",
            [FromQuery] string orderDirection = "ASC"
            )
        {
            try
            {
                var query = _context.Categories.AsNoTracking()
                    .Where(t => nameCategory == null || t.Name.Contains(nameCategory))
                    .AsQueryable();
                        
                var isDescending = orderDirection?.ToUpper() == "DESC";
        
                query = orderBy.ToLower() switch
                {
                    "name_desc" => query.OrderByDescending(c => c.Name),
                    "name" or _ => isDescending 
                        ? query.OrderByDescending(c => c.Name)
                        : query.OrderBy(c => c.Name)
                };
                var totalCount = await query.CountAsync();
                
                    var categories = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        ExternalId = c.ExternalId,
                        ItemCount = _context.Items.Count(i => i.CategoryId == c.Id && i.Ativo)
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Data = categories,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar categorias");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }
        
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
                var category = await _context.Categories
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        ExternalId = c.ExternalId,
                        ItemCount = c.Items.Count(i => i.Ativo)
                    })
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return NotFound(new { message = $"Categoria com ID {id} não encontrada" });
                }

                return Ok(category);
            
        }
        
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
        {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return BadRequest(new { message = "Nome é obrigatório" });
                }
                
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name == dto.Name);

                if (existingCategory != null)
                {
                    return Conflict(new { 
                        message = $"Já existe uma categoria com o nome '{dto.Name}'",
                        categoryId = existingCategory.Id
                    });
                }

                var category = new Category
                {
                    Name = dto.Name,
                    ExternalId = dto.ExternalId
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                var result = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    ExternalId = category.ExternalId,
                    ItemCount = 0
                };

                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, result);
        }
        
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryDto dto)
        {
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                {
                    return NotFound(new { message = $"Categoria com ID {id} não encontrada" });
                }

                category.Name = dto.Name;
                category.ExternalId = dto.ExternalId;

                await _context.SaveChangesAsync();

                return NoContent();
        }
        
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
                var category = await _context.Categories
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return NotFound(new { message = $"Categoria com ID {id} não encontrada" });
                }
                
                if (category.Items.Any(i => i.Ativo))
                {
                    return BadRequest(new { 
                        message = "Não é possível excluir categoria com itens ativos",
                        activeItemsCount = category.Items.Count(i => i.Ativo)
                    });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return NoContent();
        }
        
        [HttpGet("by-external/{externalId}")]
        public async Task<ActionResult<CategoryDto>> GetCategoryByExternalId(string externalId)
        {
                var category = await _context.Categories
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        ExternalId = c.ExternalId,
                        ItemCount = c.Items.Count(i => i.Ativo)
                    })
                    .FirstOrDefaultAsync(c => c.ExternalId == externalId);

                if (category == null)
                {
                    return NotFound(new { message = $"Categoria com ExternalId {externalId} não encontrada" });
                }

                return Ok(category);
            
        }
}