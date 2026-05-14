using Microsoft.EntityFrameworkCore;
using WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;
using WinesoftPlatform.API.Analytics.Domain.Repositories;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace WinesoftPlatform.API.Analytics.Infrastructure.Persistence.Repositories;

public class AnalyticsRepository(AppDbContext context) : IAnalyticsRepository
{
    public async Task<IEnumerable<PurchaseOrderSummary>> GetPurchaseOrdersLast7DaysAsync()
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        return await context.Orders.AsNoTracking()
            .Where(o => o.CreatedDate >= sevenDaysAgo)
            .OrderByDescending(o => o.CreatedDate)
            .Select(o => new PurchaseOrderSummary(o.Id, o.Status, o.CreatedDate.Value.DateTime, o.ProductId, o.Quantity, o.Supplier))
            .ToListAsync();
    }

    public async Task<IEnumerable<SupplyLevel>> GetSupplyLevelsAsync()
    {
        return await context.Supplies.AsNoTracking()
            .GroupBy(s => s.SupplyName)
            .Select(g => new SupplyLevel(g.Key, g.Sum(s => s.Quantity)))
            .ToListAsync();
    }

    public async Task<IEnumerable<LowStockAlert>> GetLowStockAlertsAsync(int threshold)
    {
        return await context.Supplies.AsNoTracking()
            .Where(s => s.Quantity < threshold)
            .Select(s => new LowStockAlert(s.SupplyName, s.Quantity, threshold))
            .ToListAsync();
    }

    public async Task<IEnumerable<SupplyRotationMetric>> GetSupplyRotationAsync(DateTime startDate, DateTime endDate)
    {
        //Date logic
        var end = endDate.Date.AddDays(1).AddTicks(-1);
        var start = startDate.Date;

        var rawData = await context.Supplies.AsNoTracking()
            .Where(s => s.Date >= start && s.Date <= end)
            .ToListAsync();

        return rawData.GroupBy(s => s.Date.Date)
            .Select(g => new SupplyRotationMetric(g.Key, g.Count()))
            .OrderBy(r => r.Day)
            .ToList();
    }

    public async Task<CostsSummary> GetCostsSummaryAsync(DateTime startDate, DateTime endDate)
    {
        var end = endDate.Date.AddDays(1).AddTicks(-1);
        var start = startDate.Date;

        var total = await context.Orders.AsNoTracking()
            .Where(o => o.CreatedDate >= start && o.CreatedDate <= end)
            .Join(context.Supplies, o => o.ProductId, s => s.Id, (o, s) => new { o.Quantity, s.Price })
            .SumAsync(x => (double)x.Quantity * (double)x.Price);

        return new CostsSummary(total, start, end);
    }
}