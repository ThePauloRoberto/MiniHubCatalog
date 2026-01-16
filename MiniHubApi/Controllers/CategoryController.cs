using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniHubApi.Application.DTOs;
using MiniHubApi.Application.DTOs.Responses;
using MiniHubApi.Application.Services.Interfaces;

namespace MiniHubApi.Controllers;


  [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IAuditService _auditService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            ICategoryService categoryService,
            IAuditService auditService,
            ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _auditService = auditService;
            _logger = logger;
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin,Editor,Viewer")]
        public async Task<ActionResult<PagedResponse<CategoryDto>>> GetCategories(
            [FromQuery] string? name = null,
            [FromQuery] string orderBy = "name",
            [FromQuery] string orderDirection = "ASC",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _categoryService.GetCategoriesAsync(
                name, orderBy, orderDirection, page, pageSize);
                
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            
            if (category == null)
                return NotFound(new { Message = $"Category with ID {id} not found" });
                
            return Ok(category);
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createDto)
        {
            var createdCategory = await _categoryService.CreateCategoryAsync(createDto);
            
            await _auditService.LogActionAsync(
                action: "CREATE",
                entityType: "Category",
                entityId: createdCategory.Id.ToString(),
                newValues: new { 
                    Name = createdCategory.Name,
                    ExternalId = createdCategory.ExternalId
                });
            
            return CreatedAtAction(nameof(GetCategory), new { id = createdCategory.Id }, createdCategory);
        }
        
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, CreateCategoryDto updateDto)
        {
            var updatedCategory = await _categoryService.UpdateCategoryAsync(id, updateDto);
            
            if (updatedCategory == null)
                return NotFound(new { Message = $"Category with ID {id} not found" });
            
            await _auditService.LogActionAsync(
                action: "UPDATE",
                entityType: "Category",
                entityId: id.ToString(),
                newValues: new { 
                    Name = updatedCategory.Name,
                    ExternalId = updatedCategory.ExternalId
                });
            
            return Ok(updatedCategory);
        }
        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            var deleted = await _categoryService.DeleteCategoryAsync(id);
            
            if (!deleted)
                return NotFound(new { Message = $"Category with ID {id} not found" });
            
            await _auditService.LogActionAsync(
                action: "DELETE",
                entityType: "Category",
                entityId: id.ToString(),
                newValues: new {});
            
            return NoContent();
        }
    }