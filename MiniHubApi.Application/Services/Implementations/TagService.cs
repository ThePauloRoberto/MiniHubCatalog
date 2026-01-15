using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiniHubApi.Application.DTOs;
using MiniHubApi.Application.DTOs.Responses;
using MiniHubApi.Application.Services.Interfaces;
using MiniHubApi.Domain.Entities;
using MiniHubApi.Infrastructure.Data;

namespace MiniHubApi.Application.Services.Implementations;

public class TagService : ITagService
{
     private readonly ApplicationDbContext _context;
        private readonly ILogger<TagService> _logger;

        public TagService(ApplicationDbContext context, ILogger<TagService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResponse<TagDto>> GetTagsAsync(
            string? nameTag = null,
            string orderBy = "name",
            string orderDirection = "ASC",
            int page = 1,
            int pageSize = 10)
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
                    "itemcount" => isDescending 
                        ? query.OrderByDescending(t => t.Items.Count(i => i.Ativo))
                        : query.OrderBy(t => t.Items.Count(i => i.Ativo)),
                
                    "name_desc" => query.OrderByDescending(t => t.Name),
                    "name" or _ => isDescending 
                        ? query.OrderByDescending(t => t.Name)
                        : query.OrderBy(t => t.Name)
                };

                var totalCount = await query.CountAsync();
        
                var tagsData = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new TagDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        ItemCount = t.Items.Count(i => i.Ativo)
                    })
                    .ToListAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                return new PagedResponse<TagDto>(
                    tagsData, page, pageSize, totalCount 
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tags");
                throw;
            }
        }
        

        public async Task<TagDto?> GetTagByIdAsync(Guid id)
        {
            return await _context.Tags
                .AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    ItemCount = t.Items.Count(i => i.Ativo)
                })
                .FirstOrDefaultAsync();
        }
        
        public async Task<TagDto> CreateTagAsync(CreateTagDto createDto)
        {
            try
            {
                // Verifica se já existe tag com mesmo nome
                var existingTag = await _context.Tags
                    .FirstOrDefaultAsync(t => t.Name.ToLower() == createDto.Name.ToLower());
                    
                if (existingTag != null)
                    throw new InvalidOperationException($"Tag with name '{createDto.Name}' already exists");

                var tag = new Tag
                {
                    Name = createDto.Name
                };

                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();

                return await GetTagByIdAsync(tag.Id) 
                       ?? throw new InvalidOperationException("Tag not found after creation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag");
                throw;
            }
        }
}