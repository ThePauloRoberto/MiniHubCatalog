namespace MiniHubApi.Application.Services.Interfaces;


public interface IReportService
{
    Task<DashboardReport> GetDashboardReportAsync();
    Task<string> GenerateExportFileAsync(); // Método principal para exportar
}

public class DashboardReport
{
    public int TotalItems { get; set; }
    public int TotalCategories { get; set; }
    public int TotalTags { get; set; }
    public int ActiveItems { get; set; }
    public int OutOfStockItems { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public decimal AverageItemPrice { get; set; }
}


    public class SalesReport
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal TotalValue { get; set; }
        public int ItemsSold { get; set; }
        public List<CategorySales> CategorySales { get; set; } = new();
        public List<PriceRangeDistribution> PriceDistribution { get; set; } = new();
    }

    public class InventoryReport
    {
        public int TotalItems { get; set; }
        public int ActiveItems { get; set; }
        public int InactiveItems { get; set; }
        public int LowStockItems { get; set; } // Estoque < 10
        public int OutOfStockItems { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public List<StockStatus> StockStatus { get; set; } = new();
    }

    public class CategoryReport
    {
        public int TotalCategories { get; set; }
        public List<CategoryStats> Categories { get; set; } = new();
        public CategoryDistribution Distribution { get; set; } = new();
    }

    public class AuditReport
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalAudits { get; set; }
        public Dictionary<string, int> ActionsByType { get; set; } = new();
        public Dictionary<string, int> EntitiesByType { get; set; } = new();
        public List<RecentAudit> RecentAudits { get; set; } = new();
    }

    // Classes auxiliares
    public class TopCategory
    {
        public string CategoryName { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class TopTag
    {
        public string TagName { get; set; }
        public int ItemCount { get; set; }
        public int ActiveItemCount { get; set; }
    }

    public class RecentActivity
    {
        public int ItemsCreatedLast7Days { get; set; }
        public int ItemsUpdatedLast7Days { get; set; }
        public List<string> RecentImports { get; set; } = new();
    }

    public class CategorySales
    {
        public string CategoryName { get; set; }
        public int ItemsCount { get; set; }
        public decimal TotalValue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class PriceRangeDistribution
    {
        public string Range { get; set; } // "0-50", "51-100", etc
        public int ItemCount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class StockStatus
    {
        public string Status { get; set; } // "Em estoque", "Baixo estoque", "Sem estoque"
        public int ItemCount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class CategoryStats
    {
        public string CategoryName { get; set; }
        public int ItemCount { get; set; }
        public int ActiveItemCount { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class CategoryDistribution
    {
        public string LargestCategory { get; set; }
        public string SmallestCategory { get; set; }
        public decimal DiversityIndex { get; set; } // 0-1
    }

    public class RecentAudit
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string Details { get; set; }
    }
