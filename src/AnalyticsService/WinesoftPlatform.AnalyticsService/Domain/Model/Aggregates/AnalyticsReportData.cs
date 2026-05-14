using WinesoftPlatform.API.Analytics.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Analytics.Domain.Model.Aggregates;

/// <summary>
/// Aggregate root containing analytics report data
/// </summary>
/// <remarks>
/// This aggregate encapsulates all data required for generating analytics reports,
/// including orders, supplies, and various metrics for the specified period.
/// </remarks>
public class AnalyticsReportData
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public CostsSummaryResource? CostSummary { get; }
    public IEnumerable<PurchaseOrderResource> Orders { get; }
    public IEnumerable<SupplyRotationResource> SupplyRotation { get; }
    public IEnumerable<SupplyLevelResource> SupplyLevels { get; }
    public IEnumerable<LowStockAlertResource> LowStockAlerts { get; }

    public AnalyticsReportData(
        DateTime startDate,
        DateTime endDate,
        CostsSummaryResource? costSummary,
        IEnumerable<PurchaseOrderResource> orders,
        IEnumerable<SupplyRotationResource> supplyRotation,
        IEnumerable<SupplyLevelResource> supplyLevels,
        IEnumerable<LowStockAlertResource> lowStockAlerts)
    {
        StartDate = startDate;
        EndDate = endDate;
        CostSummary = costSummary;
        Orders = orders ?? Enumerable.Empty<PurchaseOrderResource>();
        SupplyRotation = supplyRotation ?? Enumerable.Empty<SupplyRotationResource>();
        SupplyLevels = supplyLevels ?? Enumerable.Empty<SupplyLevelResource>();
        LowStockAlerts = lowStockAlerts ?? Enumerable.Empty<LowStockAlertResource>();
    }
}