using WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;

namespace WinesoftPlatform.API.Analytics.Domain.Repositories;

public interface IAnalyticsRepository
{
    Task<IEnumerable<PurchaseOrderSummary>> GetPurchaseOrdersLast7DaysAsync(int ownerId);
    Task<IEnumerable<SupplyLevel>> GetSupplyLevelsAsync(int ownerId);
    Task<IEnumerable<LowStockAlert>> GetLowStockAlertsAsync(int ownerId, int threshold);
    Task<IEnumerable<SupplyRotationMetric>> GetSupplyRotationAsync(int ownerId, DateTime startDate, DateTime endDate);
    Task<CostsSummary> GetCostsSummaryAsync(int ownerId, DateTime startDate, DateTime endDate);
}