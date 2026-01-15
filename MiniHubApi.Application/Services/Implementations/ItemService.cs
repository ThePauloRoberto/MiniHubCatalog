using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiniHubApi.Application.DTOs;
using MiniHubApi.Application.DTOs.External;
using MiniHubApi.Application.DTOs.Responses;
using MiniHubApi.Application.Services.Interfaces;
using MiniHubApi.Domain.Entities;
using MiniHubApi.Infrastructure.Data;

namespace MiniHubApi.Application.Services.Implementations
{
    public class ItemService : IItemService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ItemService> _logger;

        public ItemService(ApplicationDbContext context, ILogger<ItemService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResponse<ItemDto>> GetItemsAsync(
            string? searchTerm = null,
            string orderBy = "nome",
            string orderDirection = "ASC",
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var query = _context.Items.AsNoTracking()
                    .Include(i => i.Categoria)
                    .Include(i => i.Tags)
                    .Where(i => searchTerm == null || i.Nome.Contains(searchTerm))
                    .AsQueryable();

                var isDescending = orderDirection?.ToUpper() == "DESC";

                query = orderBy.ToLower() switch
                {
                    "descricao" => isDescending
                        ? query.OrderByDescending(i => i.Descricao)
                        : query.OrderBy(i => i.Descricao),
                    "preco" => isDescending
                        ? query.OrderByDescending(i => i.Preco)
                        : query.OrderBy(i => i.Preco),
                    "estoque" => isDescending
                        ? query.OrderByDescending(i => i.Estoque)
                        : query.OrderBy(i => i.Estoque),
                    _ => isDescending
                        ? query.OrderByDescending(i => i.Nome)
                        : query.OrderBy(i => i.Nome)
                };

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(i => new ItemDto
                    {
                        Id = i.Id,
                        Nome = i.Nome,
                        Descricao = i.Descricao,
                        Preco = i.Preco,
                        Ativo = i.Ativo,
                        Estoque = i.Estoque,
                        ExternalId = i.ExternalId,
                        CategoryId = i.CategoryId,
                        Categoria = i.Categoria != null
                            ? new CategoryDto
                            {
                                Id = i.Categoria.Id,
                                Name = i.Categoria.Name,
                                ExternalId = i.Categoria.ExternalId
                            }
                            : null,
                        Tags = i.Tags.Select(t => new TagDto
                        {
                            Id = t.Id,
                            Name = t.Name
                        }).ToList()
                    })
                    .ToListAsync();

                return new PagedResponse<ItemDto>(items, page, pageSize, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting items");
                throw;
            }
        }

        public async Task<ItemDto?> GetItemByIdAsync(Guid id)
        {
            try
            {
                return await _context.Items
                    .AsNoTracking()
                    .Include(i => i.Categoria)
                    .Include(i => i.Tags)
                    .Where(i => i.Id == id)
                    .Select(i => new ItemDto
                    {
                        Id = i.Id,
                        Nome = i.Nome,
                        Descricao = i.Descricao,
                        Preco = i.Preco,
                        Ativo = i.Ativo,
                        Estoque = i.Estoque,
                        ExternalId = i.ExternalId,
                        CategoryId = i.CategoryId,
                        Categoria = i.Categoria != null
                            ? new CategoryDto
                            {
                                Id = i.Categoria.Id,
                                Name = i.Categoria.Name,
                                ExternalId = i.Categoria.ExternalId
                            }
                            : null,
                        Tags = i.Tags.Select(t => new TagDto
                        {
                            Id = t.Id,
                            Name = t.Name
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting item {id}");
                throw;
            }
        }

        public async Task<ItemDto> CreateItemAsync(CreateItemDto createDto)
        {
            try
            {
                var item = new Item
                {
                    Id = Guid.NewGuid(),
                    Nome = createDto.Nome,
                    Descricao = createDto.Descricao,
                    Preco = createDto.Preco,
                    Ativo = createDto.Ativo,
                    Estoque = createDto.Estoque,
                    CategoryId = createDto.CategoryId,
                };

                _context.Items.Add(item);
                await _context.SaveChangesAsync();

                return await GetItemByIdAsync(item.Id)
                       ?? throw new InvalidOperationException("Item not found after creation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating item");
                throw;
            }
        }

        public async Task<ItemDto?> UpdateItemAsync(Guid id, UpdateItemDto updateDto)
        {
            try
            {
                var item = await _context.Items
                    .Include(i => i.Tags)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (item == null)
                    return null;

                // Atualiza apenas os campos que vieram no DTO
                if (!string.IsNullOrEmpty(updateDto.Nome))
                    item.Nome = updateDto.Nome;

                if (!string.IsNullOrEmpty(updateDto.Descricao))
                    item.Descricao = updateDto.Descricao;

                if (updateDto.Preco.HasValue)
                    item.Preco = updateDto.Preco.Value;

                if (updateDto.Ativo.HasValue)
                    item.Ativo = updateDto.Ativo.Value;

                if (updateDto.Estoque.HasValue)
                    item.Estoque = updateDto.Estoque.Value;

                if (updateDto.CategoryId.HasValue)
                    item.CategoryId = updateDto.CategoryId.Value;

                await _context.SaveChangesAsync();
                return await GetItemByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating item {id}");
                throw;
            }
        }

        public async Task<bool> DeleteItemAsync(Guid id)
        {
            try
            {
                var item = await _context.Items.FindAsync(id);
                if (item == null)
                    return false;

                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting item {id}");
                throw;
            }
        }

        public async Task<ItemDto> ImportItemAsync(ImportProductDto importDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(importDto.ExternalId))
                    throw new ArgumentException("ExternalId é obrigatório para importação");

                // Verifica se já existe
                var existingItem = await _context.Items
                    .FirstOrDefaultAsync(i => i.ExternalId == importDto.ExternalId);

                if (existingItem != null)
                    throw new InvalidOperationException($"Item com ExternalId {importDto.ExternalId} já existe");

                // Encontra categoria
                Category? category = null;
                if (!string.IsNullOrEmpty(importDto.CategoryExternalId))
                {
                    category = await _context.Categories
                        .FirstOrDefaultAsync(c => c.ExternalId == importDto.CategoryExternalId);
                }

                var item = new Item
                {
                    Id = Guid.NewGuid(),
                    ExternalId = importDto.ExternalId,
                    Nome = importDto.Nome,
                    Descricao = importDto.Descricao,
                    Preco = importDto.Preco,
                    Ativo = importDto.Ativo,
                    Estoque = importDto.Estoque,
                    CategoryExternalId = importDto.CategoryExternalId,
                    CategoryId = category?.Id,
                };

                // Processa tags se existirem
                if (importDto.Tags != null && importDto.Tags.Any())
                {
                    await ProcessTagsForItemAsync(item, importDto.Tags);
                }

                _context.Items.Add(item);
                await _context.SaveChangesAsync();

                return await GetItemByIdAsync(item.Id)
                       ?? throw new InvalidOperationException("Item não encontrado após criação");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao importar item {importDto?.ExternalId}");
                throw;
            }
        }

        private async Task ProcessTagsForItemAsync(Item item, List<string> tagNames)
        {
            var normalizedTags = tagNames
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!normalizedTags.Any())
                return;

            // Busca tags existentes
            var existingTags = await _context.Tags
                .Where(t => normalizedTags.Select(n => n.ToLower()).Contains(t.Name.ToLower()))
                .ToListAsync();

            var existingTagNames = existingTags
                .Select(t => t.Name.ToLower())
                .ToHashSet();

            // Cria novas tags
            var newTags = normalizedTags
                .Where(name => !existingTagNames.Contains(name.ToLower()))
                .Select(name => new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = name
                })
                .ToList();

            if (newTags.Any())
            {
                await _context.Tags.AddRangeAsync(newTags);
                await _context.SaveChangesAsync();

                // Busca as tags criadas (para ter o ID)
                var createdTags = await _context.Tags
                    .Where(t => newTags.Select(nt => nt.Name.ToLower()).Contains(t.Name.ToLower()))
                    .ToListAsync();

                existingTags.AddRange(createdTags);
            }

            // Associa todas as tags ao item
            item.Tags = existingTags;
        }
    }
}