using Microsoft.EntityFrameworkCore;
using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Repositories;
using WinesoftPlatform.InventoryService.Infrastructure.Persistence.EFC.Configuration;

namespace WinesoftPlatform.API.Inventory.Infrastructure.Persistence.Repositories;

public class SensorAlertRepository(InventoryDbContext context) : ISensorAlertRepository
{
    public async Task AddAsync(SensorAlert alert)
    {
        await context.SensorAlerts.AddAsync(alert);
    }

    public async Task<SensorAlert?> FindByIdAsync(int id)
    {
        return await context.SensorAlerts.FindAsync(id);
    }

    public async Task<IEnumerable<SensorAlert>> FindAllAsync(
        string? status, string? sensorType, int page, int size)
    {
        var query = context.SensorAlerts.AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(a => a.Status == status);

        if (!string.IsNullOrEmpty(sensorType))
            query = query.Where(a => a.SensorType == sensorType);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string? status, string? sensorType)
    {
        var query = context.SensorAlerts.AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(a => a.Status == status);

        if (!string.IsNullOrEmpty(sensorType))
            query = query.Where(a => a.SensorType == sensorType);

        return await query.CountAsync();
    }

    public void Update(SensorAlert alert)
    {
        context.SensorAlerts.Update(alert);
    }
}
