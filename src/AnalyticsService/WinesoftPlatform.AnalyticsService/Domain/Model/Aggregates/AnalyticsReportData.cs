using WinesoftPlatform.API.Analytics.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Analytics.Domain.Model.Aggregates;

/// <summary>
/// Aggregate root containing analytics report data
/// </summary>
/// <remarks>
/// This aggregate encapsulates all data required for generating analytics reports,
/// including supplies and various metrics for the specified period.
/// </remarks>
public class AnalyticsReportData
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public IEnumerable<SupplyRotationResource> SupplyRotation { get; }
    public IEnumerable<SupplyLevelResource> SupplyLevels { get; }
    public IEnumerable<LowStockAlertResource> LowStockAlerts { get; }

    public AnalyticsReportData(
        DateTime startDate,
        DateTime endDate,
        IEnumerable<SupplyRotationResource> supplyRotation,
        IEnumerable<SupplyLevelResource> supplyLevels,
        IEnumerable<LowStockAlertResource> lowStockAlerts)
    {
        StartDate = startDate;
        EndDate = endDate;
        SupplyRotation = supplyRotation ?? Enumerable.Empty<SupplyRotationResource>();
        SupplyLevels = supplyLevels ?? Enumerable.Empty<SupplyLevelResource>();
        LowStockAlerts = lowStockAlerts ?? Enumerable.Empty<LowStockAlertResource>();
    }
}