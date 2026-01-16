using Microsoft.AspNetCore.Mvc;
using MiniHubApi.Application.Services.Interfaces;

namespace MiniHubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IReportService reportService,
        ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardReport>> GetDashboardReport()
    {
        try
        {
            var report = await _reportService.GetDashboardReportAsync();
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard report");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportReport()
    {
        try
        {
            var filePath = await _reportService.GenerateExportFileAsync();
            var fileName = Path.GetFileName(filePath);
            
            return Ok(new 
            { 
                Message = "Export generated successfully",
                FileName = fileName,
                DownloadUrl = $"/api/reports/download/{fileName}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating export");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> DownloadReport(string fileName)
    {
        try
        {
            var filePath = Path.Combine("Exports", fileName);
            
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { Message = $"File '{fileName}' not found" });
            
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var contentType = "application/json";
            
            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {FileName}", fileName);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            var report = await _reportService.GetDashboardReportAsync();
            
            var summary = new
            {
                report.TotalItems,
                report.ActiveItems,
                report.TotalInventoryValue,
                report.AverageItemPrice,
                GeneratedAt = DateTime.UtcNow
            };
            
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting summary");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }
}