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

        public CategoryService(ApplicationDbContext context, ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
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

                if (!string.IsNullOrEmpty(updateDto.Name))
                    category.Name = updateDto.Name;
                    
                category.ExternalId = updateDto.ExternalId; // Pode ser null

                await _context.SaveChangesAsync();
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

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
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