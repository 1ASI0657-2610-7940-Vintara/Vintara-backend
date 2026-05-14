using Microsoft.EntityFrameworkCore;
using WinesoftPlatform.API.Analytics.Domain.Model.Queries;
using WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;
using WinesoftPlatform.API.Analytics.Domain.Repositories;
using WinesoftPlatform.API.Analytics.Domain.Services;
using WinesoftPlatform.API.Analytics.Interfaces.REST.Resources;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace WinesoftPlatform.API.Analytics.Application.Internal.QueryServices;

/// <summary>
/// Query service for analytics operations.
/// </summary>
/// <param name="context">
/// The <see cref="AppDbContext"/> database context.
/// </param>
public class AnalyticsQueryService(IAnalyticsRepository analyticsRepository) : IAnalyticsQueryService
{
    /// <inheritdoc />
    public async Task<IEnumerable<PurchaseOrderSummary>> Handle(GetPurchaseOrdersLast7DaysQuery query)
    {
        return await analyticsRepository.GetPurchaseOrdersLast7DaysAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SupplyLevel>> Handle(GetAllSupplyLevelsQuery query)
    {
        return await analyticsRepository.GetSupplyLevelsAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LowStockAlert>> Handle(GetLowStockAlertsQuery query)
    {
        return await analyticsRepository.GetLowStockAlertsAsync(query.Threshold);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SupplyRotationMetric>> Handle(GetSupplyRotationQuery query)
    {
        var endDate = query.EndDate ?? DateTime.UtcNow;
        var startDate = query.StartDate ?? endDate.AddDays(-7);
        return await analyticsRepository.GetSupplyRotationAsync(startDate, endDate);
    }

    /// <inheritdoc />
    public async Task<CostsSummary> Handle(GetInventoryKpisQuery query)
    {
        var endDate = query.EndDate ?? DateTime.UtcNow;
        var startDate = query.StartDate ?? endDate.AddDays(-30);
        return await analyticsRepository.GetCostsSummaryAsync(startDate, endDate);
    }
}