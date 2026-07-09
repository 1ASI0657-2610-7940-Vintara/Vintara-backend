using WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;

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
    public IEnumerable<SupplyRotationMetric> SupplyRotation { get; }
    public IEnumerable<SupplyLevel> SupplyLevels { get; }
    public IEnumerable<LowStockAlert> LowStockAlerts { get; }

    public AnalyticsReportData(
        DateTime startDate,
        DateTime endDate,
        IEnumerable<SupplyRotationMetric> supplyRotation,
        IEnumerable<SupplyLevel> supplyLevels,
        IEnumerable<LowStockAlert> lowStockAlerts)
    {
        StartDate = startDate;
        EndDate = endDate;
        SupplyRotation = supplyRotation ?? Enumerable.Empty<SupplyRotationMetric>();
        SupplyLevels = supplyLevels ?? Enumerable.Empty<SupplyLevel>();
        LowStockAlerts = lowStockAlerts ?? Enumerable.Empty<LowStockAlert>();
    }
}