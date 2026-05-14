using WinesoftPlatform.API.Analytics.Domain.Model.Queries;
using WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;
using WinesoftPlatform.API.Analytics.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Analytics.Domain.Services;

/// <summary>
/// Service for handling analytics queries.
/// </summary>
public interface IAnalyticsQueryService
{
    /// <summary>
    /// Handles the query to retrieve purchase orders from the last 7 days.
    /// </summary>
    /// <param name="query">The <see cref="GetPurchaseOrdersLast7DaysQuery"/> query to process.</param>
    Task<IEnumerable<PurchaseOrderSummary>> Handle(GetPurchaseOrdersLast7DaysQuery query);
    
    /// <summary>
    /// Retrieves current supply levels for all products.
    /// </summary>
    /// <returns>A collection of <see cref="SupplyLevelResource"/> objects.</returns>
    Task<IEnumerable<SupplyLevel>> Handle(GetAllSupplyLevelsQuery query);
    
    /// <summary>
    /// Retrieves low stock alerts for products below threshold.
    /// </summary>
    /// <returns>A collection of <see cref="LowStockAlertResource"/> objects.</returns>
    Task<IEnumerable<LowStockAlert>> Handle(GetLowStockAlertsQuery query);
    /// <summary>
    /// Retrieves supply rotation data for the specified period.
    /// </summary>
    /// <param name="query">The <see cref="GetAnalyticsMetricsQuery"/> with date range parameters.</param>
    /// <returns>A collection of <see cref="SupplyRotationResource"/> objects.</returns>
    Task<IEnumerable<SupplyRotationMetric>> Handle(GetSupplyRotationQuery query);
    
    /// <summary>
    /// Retrieves costs summary for the specified period.
    /// </summary>
    /// <param name="query">The <see cref="GetAnalyticsMetricsQuery"/> with date range parameters.</param>
    /// <returns>A <see cref="CostsSummaryResource"/> with total costs.</returns>
    Task<CostsSummary> Handle(GetInventoryKpisQuery query);
}