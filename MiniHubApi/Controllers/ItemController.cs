using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniHubApi.Application.DTOs;
using MiniHubApi.Application.DTOs.External;
using MiniHubApi.Application.DTOs.Responses;
using MiniHubApi.Application.Services.Interfaces;
using MiniHubApi.Application.Utils;
using MiniHubApi.Domain.Entities;
using MiniHubApi.Infrastructure.Data;

namespace MiniHubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemController : ControllerBase
{
  private readonly IItemService _itemService;
  private readonly ILogger<ItemController> _logger;

  public ItemController(IItemService itemService, ILogger<ItemController> logger)
  {
      _itemService = itemService;
    _logger = logger;
  }
  
  [HttpGet]
  public async Task<ActionResult<PagedResponse<ItemDto>>> GetItems([FromQuery] ItemQueryParams queryParams)
  {
          var result = await _itemService.GetItemsAsync(
              queryParams.Search,
              queryParams.OrderBy,
              queryParams.OrderDirection,
              queryParams.Page,
              queryParams.PageSize);
          return Ok(result);
  }
  
  [HttpGet("{id}")]
  public async Task<ActionResult<ItemDto>> GetItem(Guid id)
  {
          var item = await _itemService.GetItemByIdAsync(id);
          return item == null ? NotFound() : Ok(item);
  }
    
  [HttpPost]
  public async Task<ActionResult<ItemDto>> CreateItem(CreateItemDto createDto)
  {
          var createdItem = await _itemService.CreateItemAsync(createDto);
          return CreatedAtAction(nameof(GetItem), new { id = createdItem.Id }, createdItem);
  }
    
   
  [HttpPut("{id}")]
  public async Task<ActionResult<ItemDto>> UpdateItem(Guid id, UpdateItemDto updateDto)
  {
          var updatedItem = await _itemService.UpdateItemAsync(id, updateDto);
          return updatedItem == null ? NotFound() : Ok(updatedItem);
  }
    
  [HttpDelete("{id}")]
  public async Task<ActionResult> DeleteItem(Guid id)
  {

          var deleted = await _itemService.DeleteItemAsync(id);
          return deleted ? NoContent() : NotFound();
  }
  
  [HttpPost("import")]
  public async Task<ActionResult<ItemDto>> ImportProduct([FromBody] ImportProductDto importDto)
  {
          if (!ModelState.IsValid)
              return BadRequest(ModelState);

          var importedItem = await _itemService.ImportItemAsync(importDto);
          return CreatedAtAction(nameof(GetItem), new { id = importedItem.Id }, importedItem);
  }
}