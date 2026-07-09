using Microsoft.EntityFrameworkCore;
using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Repositories;
using WinesoftPlatform.InventoryService.Infrastructure.Persistence.EFC.Configuration;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace WinesoftPlatform.API.Inventory.Infrastructure.Persistence.Repositories;

public class StockMovementRepository(InventoryDbContext context) : BaseRepository<StockMovement>(context), IStockMovementRepository
{
    public async Task<IEnumerable<StockMovement>> ListByOwnerIdAsync(int ownerId)
    {
        return await Context.Set<StockMovement>()
            .Include(m => m.Supply)
            .Where(m => m.OwnerId == ownerId)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockMovement>> ListBySupplyIdAndOwnerIdAsync(int supplyId, int ownerId)
    {
        return await Context.Set<StockMovement>()
            .Include(m => m.Supply)
            .Where(m => m.SupplyId == supplyId && m.OwnerId == ownerId)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }
}
