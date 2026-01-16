using Microsoft.AspNetCore.Mvc;
using MiniHubApi.Application.DTOs;
using MiniHubApi.Application.DTOs.Responses;
using MiniHubApi.Application.Services.Interfaces;
using MiniHubApi.Application.Utils;

namespace MiniHubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly IAuditService _auditService;
        private readonly ILogger<TagsController> _logger;

        public TagsController(ITagService tagService, IAuditService auditService,ILogger<TagsController> logger)
        {
            _tagService = tagService;
            _logger = logger;
            _auditService = auditService;
        }
        
        [HttpGet]
        public async Task<ActionResult<PagedResponse<TagDto>>> GetTags([FromQuery] TagQueryParams queryParams)
        {
                var result = await _tagService.GetTagsAsync(
                    queryParams.Name, queryParams.OrderBy, queryParams.OrderDirection, queryParams.Page, queryParams.PageSize);
                
                return Ok(result);
        }
        
        [HttpPost]
        public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto createDto)
        {
            var createdTag = await _tagService.CreateTagAsync(createDto);
            
            await _auditService.LogActionAsync(
                action: "CREATE",
                entityType: "Tag",
                entityId: createdTag.Id.ToString(),
                newValues: new { 
                    Name = createdTag.Name
                });
            
            return CreatedAtAction(createdTag.Id.ToString(), createdTag);
        }
}