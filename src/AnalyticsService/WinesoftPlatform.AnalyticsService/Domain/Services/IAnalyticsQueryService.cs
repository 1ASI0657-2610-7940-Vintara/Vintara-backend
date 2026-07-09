using WinesoftPlatform.API.Analytics.Domain.Model.Queries;
using WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;

namespace WinesoftPlatform.API.Analytics.Domain.Services;

/// <summary>
/// Service for handling analytics queries.
/// </summary>
public interface IAnalyticsQueryService
{
    /// <summary>
    /// Retrieves current supply levels for all products.
    /// </summary>
    Task<IEnumerable<SupplyLevel>> Handle(GetAllSupplyLevelsQuery query);
    
    /// <summary>
    /// Retrieves low stock alerts for products below threshold.
    /// </summary>
    Task<IEnumerable<LowStockAlert>> Handle(GetLowStockAlertsQuery query);

    /// <summary>
    /// Retrieves supply rotation data for the specified period.
    /// </summary>
    /// <param name="query">The <see cref="GetSupplyRotationQuery"/> with date range parameters.</param>
    Task<IEnumerable<SupplyRotationMetric>> Handle(GetSupplyRotationQuery query);
}