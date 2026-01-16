using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiniHubApi.Application.DTOs;
using MiniHubApi.Application.DTOs.Responses;
using MiniHubApi.Application.Services.Interfaces;
using MiniHubApi.Domain.Entities;
using MiniHubApi.Infrastructure.Data;

namespace MiniHubApi.Application.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryService> _logger;
        private readonly IAuditService _auditLogger;

        public CategoryService(ApplicationDbContext context, ILogger<CategoryService> logger, IAuditService auditLogger)
        {
            _context = context;
            _logger = logger;
            _auditLogger = auditLogger;
        }

        public async Task<PagedResponse<CategoryDto>> GetCategoriesAsync(
            string? nameCategory = null,
            string orderBy = "name",
            string orderDirection = "ASC",
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var query = _context.Categories.AsNoTracking()
                    .Where(c => nameCategory == null || c.Name.Contains(nameCategory))
                    .AsQueryable();

                var isDescending = orderDirection?.ToUpper() == "DESC";

                query = orderBy.ToLower() switch
                {
                    "itemcount" => isDescending 
                        ? query.OrderByDescending(c => c.Items.Count())
                        : query.OrderBy(c => c.Items.Count()),
                    _ => isDescending 
                        ? query.OrderByDescending(c => c.Name)
                        : query.OrderBy(c => c.Name)
                };

                var totalCount = await query.CountAsync();

                var categories = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        ExternalId = c.ExternalId,
                        ItemCount = c.Items.Count()
                    })
                    .ToListAsync();

                return new PagedResponse<CategoryDto>(categories, page, pageSize, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                throw;
            }
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            try
            {
                return await _context.Categories
                    .AsNoTracking()
                    .Where(c => c.Id == id)
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        ExternalId = c.ExternalId,
                        ItemCount = c.Items.Count()
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting category {id}");
                throw;
            }
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto)
        {
            try
            {
                var category = new Category
                {
                    Name = createDto.Name,
                    ExternalId = createDto.ExternalId
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                
                await _auditLogger.LogActionAsync(
                    action: "CREATE",
                    entityType: "Category",
                    entityId: category.Id.ToString(),
                    newValues: createDto,
                    details: $"Categoria '{category.Name}' criada"
                );

                return await GetCategoryByIdAsync(category.Id) 
                    ?? throw new InvalidOperationException("Category not found after creation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                throw;
            }
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, CreateCategoryDto updateDto)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return null;
                
                var oldValues = new
                {
                    category.Name,
                    category.ExternalId
                };

                bool hasChanges = false;

                if (!string.IsNullOrEmpty(updateDto.Name) && updateDto.Name != category.Name)
                {
                    category.Name = updateDto.Name;
                    hasChanges = true;
                }

                if (updateDto.ExternalId != category.ExternalId)
                {
                    category.ExternalId = updateDto.ExternalId;
                    hasChanges = true;
                }

                if (!hasChanges)
                {
                    return await GetCategoryByIdAsync(id);
                }

                await _context.SaveChangesAsync();
                
                await _auditLogger.LogActionAsync(
                    action: "UPDATE",
                    entityType: "Category",
                    entityId: id.ToString(),
                    newValues: new { OldValues = oldValues, NewValues = updateDto },
                    details: $"Categoria '{category.Name}' atualizada"
                );

                return await GetCategoryByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating category {id}");
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                    return false;

                if (category.Items.Any())
                    throw new InvalidOperationException("Cannot delete category with associated items");
                
                var categoryInfo = new
                {
                    category.Name,
                    category.ExternalId,
                    ItemCount = category.Items.Count
                };

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                
                await _auditLogger.LogActionAsync(
                    action: "DELETE",
                    entityType: "Category",
                    entityId: id.ToString(),
                    newValues: categoryInfo,
                    details: $"Categoria '{categoryInfo.Name}' excluída"
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting category {id}");
                throw;
            }
        }
    }
}