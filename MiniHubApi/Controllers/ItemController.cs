using Microsoft.AspNetCore.Mvc;
using MiniHubApi.Application.DTOs;
using MiniHubApi.Application.DTOs.External;
using MiniHubApi.Application.DTOs.Responses;
using MiniHubApi.Application.Services.Interfaces;
using MiniHubApi.Application.Utils;

namespace MiniHubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemController : ControllerBase
{
  private readonly IItemService _itemService;
  private readonly IAuditService _auditService;
  private readonly ILogger<ItemController> _logger;

  public ItemController(IItemService itemService,IAuditService auditService, ILogger<ItemController> logger)
  {
      _itemService = itemService;
      _auditService = auditService;
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
          await _auditService.LogActionAsync(
                  action: "CREATE",
                  entityType: "Item",
                  entityId: createdItem.Id.ToString(),
                  newValues: new { 
                          Nome = createdItem.Nome,
                          Preco = createdItem.Preco,
                          Ativo = createdItem.Ativo
                  });
          return CreatedAtAction(nameof(GetItem), new { id = createdItem.Id }, createdItem);
  }
    
   
  [HttpPut("{id}")]
  public async Task<ActionResult<ItemDto>> UpdateItem(Guid id, UpdateItemDto updateDto)
  {
          var updatedItem = await _itemService.UpdateItemAsync(id, updateDto);
          
          await _auditService.LogActionAsync(
                  action: "UPDATE",
                  entityType: "Item",
                  entityId: id.ToString(),
                  newValues: new { 
                          Nome = updatedItem.Nome,
                          Preco = updatedItem.Preco,
                          Ativo = updatedItem.Ativo
                  });
          return updatedItem == null ? NotFound() : Ok(updatedItem);
  }
    
  [HttpDelete("{id}")]
  public async Task<ActionResult> DeleteItem(Guid id)
  {

          var deleted = await _itemService.DeleteItemAsync(id);
          await _auditService.LogActionAsync(
                  action: "DELETE",
                  entityType: "Item",
                  entityId: id.ToString(),
                  newValues: new {});
          return deleted ? NoContent() : NotFound();
  }
  
  [HttpPost("import")]
  public async Task<ActionResult<ItemDto>> ImportProduct([FromBody] ImportProductDto importDto)
  {
          if (!ModelState.IsValid)
              return BadRequest(ModelState);

          var importedItem = await _itemService.ImportItemAsync(importDto);
          
          await _auditService.LogActionAsync(
                  action: "IMPORT",
                  entityType: "Item",
                  entityId: importedItem.Id.ToString(),
                  newValues: new { 
                          ExternalId = importDto.ExternalId,
                          Nome = importedItem.Nome,
                          TagsCount = importDto.Tags?.Count ?? 0
                  });
          return CreatedAtAction(nameof(GetItem), new { id = importedItem.Id }, importedItem);
  }
}