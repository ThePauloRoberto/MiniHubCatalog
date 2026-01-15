using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniHubApi.Application.DTOs;
using MiniHubApi.Application.DTOs.Responses;
using MiniHubApi.Application.Services.Implementations;
using MiniHubApi.Application.Services.Interfaces;
using MiniHubApi.Application.Utils;
using MiniHubApi.Domain.Entities;
using MiniHubApi.Infrastructure.Data;

namespace MiniHubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;
        private readonly ILogger<TagsController> _logger;

        public TagsController(ITagService tagService, ILogger<TagsController> logger)
        {
            _tagService = tagService;
            _logger = logger;
        }
        
        [HttpGet]
        public async Task<ActionResult<PagedResponse<TagDto>>> GetTags([FromQuery] TagQueryParams queryParams)
        {
            try
            {
                var result = await _tagService.GetTagsAsync(
                    queryParams.Name,
                    queryParams.OrderBy,
                    queryParams.OrderDirection,
                    queryParams.Page,
                    queryParams.PageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tags");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }
        
        [HttpPost]
        public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto createDto)
        {
                var createdTag = await _tagService.CreateTagAsync(createDto);
                return CreatedAtAction(createdTag.Id.ToString(), createdTag);
        }
}