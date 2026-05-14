using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WinesoftPlatform.API.Analytics.Domain.Model.Queries;
using WinesoftPlatform.API.Analytics.Domain.Services;
using WinesoftPlatform.API.Analytics.Interfaces.REST.Resources;
using WinesoftPlatform.API.Analytics.Interfaces.REST.Transform;

namespace WinesoftPlatform.API.Analytics.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[SwaggerTag("Analytics endpoints for metrics and reports")]
public class AnalyticsController(
    IAnalyticsQueryService analyticsQueryService,
    IAnalyticsCommandService analyticsCommandService) : ControllerBase
{
    [HttpGet("last-week-purchase-orders")]
    [SwaggerOperation(
        Summary = "Get purchase orders from last week",
        Description = "Retrieves all purchase orders created in the last 7 days",
        OperationId = "GetPurchaseOrdersLast7Days")]
    [SwaggerResponse(StatusCodes.Status200OK, "Purchase orders retrieved successfully", typeof(IEnumerable<PurchaseOrderResource>))]
    public async Task<IActionResult> GetPurchaseOrdersLast7Days()
    {
        var query = new GetPurchaseOrdersLast7DaysQuery();
        var orders = await analyticsQueryService.Handle(query);
        var resources = orders.Select(PurchaseOrderResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("supply-levels")]
    [SwaggerOperation(
        Summary = "Get current supply levels",
        Description = "Retrieves current inventory levels for all supplies",
        OperationId = "GetSupplyLevels")]
    [SwaggerResponse(200, "Supply levels retrieved successfully", typeof(IEnumerable<SupplyLevelResource>))]
    public async Task<IActionResult> GetSupplyLevels()
    {
        var query = new GetAllSupplyLevelsQuery();
        var levels = await analyticsQueryService.Handle(query);
        var resources = levels.Select(SupplyLevelResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("low-stock-alerts")]
    [SwaggerOperation(
        Summary = "Get low stock alerts",
        Description = "Retrieves alerts for supplies below minimum stock threshold",
        OperationId = "GetLowStockAlerts")]
    [SwaggerResponse(200, "Low stock alerts retrieved successfully", typeof(IEnumerable<LowStockAlertResource>))]
    public async Task<IActionResult> GetLowStockAlerts()
    {
        var query = new GetLowStockAlertsQuery();
        var alerts = await analyticsQueryService.Handle(query);
        var resources = alerts.Select(LowStockAlertResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("supply-rotation-metrics")]
    [SwaggerOperation(
        Summary = "Get supply rotation metrics",
        Description = "Retrieves daily supply rotation metrics for the specified date range",
        OperationId = "GetSupplyRotation")]
    [SwaggerResponse(200, "Supply rotation data retrieved successfully", typeof(IEnumerable<SupplyRotationResource>))]
    public async Task<IActionResult> GetSupplyRotation([FromQuery] GetAnalyticsMetricsQuery metricsQuery)
    {
        var query = new GetSupplyRotationQuery(metricsQuery.StartDate, metricsQuery.EndDate);
        var data = await analyticsQueryService.Handle(query);
        var resources = data.Select(SupplyRotationResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("inventory-kpis")]
    [SwaggerOperation(
        Summary = "Get inventory KPIs",
        Description = "Retrieves total costs summary and other KPIs for the specified date range",
        OperationId = "GetCostsSummary")]
    [SwaggerResponse(200, "Inventory KPIs retrieved successfully", typeof(CostsSummaryResource))]
    public async Task<IActionResult> GetCostsSummary([FromQuery] GetAnalyticsMetricsQuery metricsQuery)
    {
        var query = new GetInventoryKpisQuery(metricsQuery.StartDate, metricsQuery.EndDate);
        var data = await analyticsQueryService.Handle(query);
        var resource = CostsSummaryResourceFromEntityAssembler.ToResourceFromEntity(data);
        return Ok(resource);
    }

    [HttpPost("reports")]
    [SwaggerOperation(
        Summary = "Generate analytics report",
        Description = "Generates a PDF report for the specified period and widgets",
        OperationId = "GenerateAnalyticsReport")]
    [SwaggerResponse(200, "Report generated successfully", typeof(FileContentResult))]
    [SwaggerResponse(400, "Invalid request parameters")]
    public async Task<IActionResult> GenerateReport([FromBody] GenerateReportResource resource)
    {
        var command = GenerateAnalyticsReportCommandFromResourceAssembler.ToCommandFromResource(resource);

        try
        {
            var pdfBytes = await analyticsCommandService.Handle(command);

            return File(pdfBytes, "application/pdf", $"analytics-report-{DateTime.UtcNow:yyyyMMdd}.pdf");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
