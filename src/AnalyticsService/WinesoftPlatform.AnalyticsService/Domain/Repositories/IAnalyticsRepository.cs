using WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;

namespace WinesoftPlatform.API.Analytics.Domain.Repositories;

public interface IAnalyticsRepository
{
    Task<IEnumerable<PurchaseOrderSummary>> GetPurchaseOrdersLast7DaysAsync();
    Task<IEnumerable<SupplyLevel>> GetSupplyLevelsAsync();
    Task<IEnumerable<LowStockAlert>> GetLowStockAlertsAsync(int threshold);
    Task<IEnumerable<SupplyRotationMetric>> GetSupplyRotationAsync(DateTime startDate, DateTime endDate);
    Task<CostsSummary> GetCostsSummaryAsync(DateTime startDate, DateTime endDate);
}