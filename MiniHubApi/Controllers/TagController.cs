using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniHubApi.Application.DTOs;
using MiniHubApi.Domain.Entities;
using MiniHubApi.Infrastructure.Data;

namespace MiniHubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
      private readonly ApplicationDbContext _context;
        private readonly ILogger<TagsController> _logger;

        public TagsController(ApplicationDbContext context, ILogger<TagsController> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagDto>>> GetTags(
            [FromQuery] string? nameTag,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string orderBy = "name",
            [FromQuery] string orderDirection = "ASC"
            )
        {
            try
            {
                var query = _context.Tags.AsNoTracking()
                    .Include(t => t.Items)
                    .Where(t => nameTag == null || t.Name.Contains(nameTag))
                    .AsQueryable();

                var isDescending = orderDirection?.ToUpper() == "DESC";
        
                query = orderBy.ToLower() switch
                {
                    "name_desc" => query.OrderByDescending(t => t.Name),
                    "name" or _ => isDescending 
                        ? query.OrderByDescending(t => t.Name)
                        : query.OrderBy(t => t.Name)
                };
                var totalCount = await query.CountAsync();
                
                    var tags = await query
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(t => new TagDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        ItemCount = t.Items.Count(i => i.Ativo)
                    })
                    .ToListAsync();

                return Ok(new
                {
                    data = tags,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar tags");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }
        
        [HttpPost]
        public async Task<ActionResult<TagDto>> CreateTag([FromBody] CreateTagDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return BadRequest(new { message = "Nome é obrigatório" });
                }
                
                var existingTag = await _context.Tags
                    .FirstOrDefaultAsync(t => t.Name == dto.Name);

                if (existingTag != null)
                {
                    return Conflict(new { 
                        message = $"Já existe uma tag com o nome '{dto.Name}'",
                        tagId = existingTag.Id
                    });
                }

                var tag = new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name
                };

                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();

                var result = new TagDto
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    ItemCount = 0
                };

                return CreatedAtAction(nameof(GetTags), new { id = tag.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar tag");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }
}