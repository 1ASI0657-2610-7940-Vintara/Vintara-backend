using WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;
using WinesoftPlatform.API.Analytics.Domain.Repositories;
using WinesoftPlatform.AnalyticsService.Infrastructure.ExternalServices;

namespace WinesoftPlatform.API.Analytics.Infrastructure.Persistence.Repositories;

public class AnalyticsRepository(
    IInventoryServiceClient inventoryClient) : IAnalyticsRepository
{
    public async Task<IEnumerable<SupplyLevel>> GetSupplyLevelsAsync(int ownerId)
    {
        var supplies = await inventoryClient.GetAllSuppliesAsync();
        return supplies
            .Where(s => s.OwnerId == ownerId)
            .GroupBy(s => s.SupplyName)
            .Select(g => new SupplyLevel(g.Key, g.Sum(s => s.Quantity)))
            .ToList();
    }

    public async Task<IEnumerable<LowStockAlert>> GetLowStockAlertsAsync(int ownerId, int threshold)
    {
        var supplies = await inventoryClient.GetAllSuppliesAsync();
        return supplies
            .Where(s => s.OwnerId == ownerId && s.Quantity < threshold)
            .Select(s => new LowStockAlert(s.SupplyName, s.Quantity, threshold))
            .ToList();
    }

    public async Task<IEnumerable<SupplyRotationMetric>> GetSupplyRotationAsync(int ownerId, DateTime startDate, DateTime endDate)
    {
        var end = endDate.Date.AddDays(1).AddTicks(-1);
        var start = startDate.Date;

        var supplies = await inventoryClient.GetAllSuppliesAsync();
        var rawData = supplies.Where(s => s.OwnerId == ownerId && s.Date >= start && s.Date <= end).ToList();

        return rawData.GroupBy(s => s.Date.Date)
            .Select(g => new SupplyRotationMetric(g.Key, g.Count()))
            .OrderBy(r => r.Day)
            .ToList();
    }
}