using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiniHubApi.Application.Services.Interfaces;
using MiniHubApi.Infrastructure.Data;
using MongoDB.Driver;

namespace MiniHubApi.Application.Services.Implementations;

  public class SimpleReportService : IReportService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly ILogger<SimpleReportService> _logger;

        public SimpleReportService(
            ApplicationDbContext dbContext,
            IMongoDatabase mongoDatabase,
            ILogger<SimpleReportService> logger)
        {
            _dbContext = dbContext;
            _mongoDatabase = mongoDatabase;
            _logger = logger;
        }

        public async Task<DashboardReport> GetDashboardReportAsync()
        {
            var report = new DashboardReport();
            
            try
            {
                // Estatísticas básicas - SIMPLES
                report.TotalItems = await _dbContext.Items.CountAsync();
                report.TotalCategories = await _dbContext.Categories.CountAsync();
                report.TotalTags = await _dbContext.Tags.CountAsync();
                report.ActiveItems = await _dbContext.Items.CountAsync(i => i.Ativo);
                report.OutOfStockItems = await _dbContext.Items.CountAsync(i => i.Estoque == 0);
                
                // Valor total do inventário
                report.TotalInventoryValue = await _dbContext.Items
                    .Where(i => i.Ativo)
                    .SumAsync(i => i.Preco * i.Estoque);
                
                // Preço médio
                report.AverageItemPrice = report.TotalItems > 0 
                    ? await _dbContext.Items.AverageAsync(i => i.Preco)
                    : 0;
                
                _logger.LogInformation("Dashboard report generated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard report");
            }
            
            return report;
        }

        public async Task<string> GenerateExportFileAsync()
        {
            try
            {
                // Dados básicos para exportação
                var exportData = new
                {
                    GeneratedAt = DateTime.UtcNow,
                    Items = await GetItemsForExportAsync(),
                    Categories = await GetCategoriesForExportAsync(),
                    Tags = await GetTagsForExportAsync(),
                    Statistics = await GetBasicStatisticsAsync()
                };

                // Converter para JSON
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(exportData, options);

                // Criar diretório se não existir
                var exportDir = Path.Combine(Directory.GetCurrentDirectory(), "Exports");
                if (!Directory.Exists(exportDir))
                    Directory.CreateDirectory(exportDir);

                // Salvar arquivo
                var fileName = $"Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
                var filePath = Path.Combine(exportDir, fileName);
                
                await File.WriteAllTextAsync(filePath, json);
                
                _logger.LogInformation("Export file created: {FileName}", fileName);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating export file");
                throw;
            }
        }

        private async Task<List<object>> GetItemsForExportAsync()
        {
            return await _dbContext.Items
                .AsNoTracking()
                .Where(i => i.Ativo) // Só itens ativos
                .Select(i => new 
                {
                    i.Id,
                    i.Nome,
                    i.Descricao,
                    i.Preco,
                    i.Estoque,
                    i.ExternalId,
                    CategoryName = i.Categoria != null ? i.Categoria.Name : "Sem categoria",
                    Tags = i.Tags.Select(t => t.Name).ToList()
                })
                .Cast<object>()
                .ToListAsync();
        }

        private async Task<List<object>> GetCategoriesForExportAsync()
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .Select(c => new 
                {
                    c.Id,
                    c.Name,
                    c.ExternalId,
                    ItemCount = c.Items.Count
                })
                .Cast<object>()
                .ToListAsync();
        }

        private async Task<List<object>> GetTagsForExportAsync()
        {
            return await _dbContext.Tags
                .AsNoTracking()
                .Select(t => new 
                {
                    t.Id,
                    t.Name,
                    ActiveItemCount = t.Items.Count(i => i.Ativo)
                })
                .Cast<object>()
                .ToListAsync();
        }

        private async Task<object> GetBasicStatisticsAsync()
        {
            var items = await _dbContext.Items.ToListAsync();
            var activeItems = items.Where(i => i.Ativo).ToList();
            
            return new
            {
                TotalItems = items.Count,
                ActiveItems = activeItems.Count,
                OutOfStockItems = items.Count(i => i.Estoque == 0),
                TotalInventoryValue = activeItems.Sum(i => i.Preco * i.Estoque),
                AveragePrice = activeItems.Any() ? activeItems.Average(i => i.Preco) : 0,
                Top3MostExpensive = items
                    .OrderByDescending(i => i.Preco)
                    .Take(3)
                    .Select(i => new { i.Nome, i.Preco })
                    .ToList()
            };
        }
        
        public Task<SalesReport> GetSalesReportAsync(DateTime? startDate = null, DateTime? endDate = null)
            => Task.FromResult(new SalesReport());

        public Task<InventoryReport> GetInventoryReportAsync()
            => Task.FromResult(new InventoryReport());

        public Task<CategoryReport> GetCategoryReportAsync()
            => Task.FromResult(new CategoryReport());

        public Task<AuditReport> GetAuditReportAsync(int days = 30)
            => Task.FromResult(new AuditReport());

        public Task<string> GenerateReportJsonAsync(string reportType, object parameters = null)
            => Task.FromResult(string.Empty);
    }