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
        public async Task<ActionResult<IEnumerable<TagDto>>> GetTags()
        {
            try
            {
                var tags = await _context.Tags
                    .Select(t => new TagDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        ItemCount = t.Items.Count(i => i.Ativo)
                    })
                    .OrderBy(t => t.Name)
                    .ToListAsync();

                return Ok(tags);
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

                // Verifica se já existe tag com esse nome
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