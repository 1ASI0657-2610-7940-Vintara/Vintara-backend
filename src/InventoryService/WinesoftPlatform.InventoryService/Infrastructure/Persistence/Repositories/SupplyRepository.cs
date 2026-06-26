using Microsoft.EntityFrameworkCore;
using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Repositories;
using WinesoftPlatform.InventoryService.Infrastructure.Persistence.EFC.Configuration;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace WinesoftPlatform.API.Inventory.Infrastructure.Persistence.Repositories;

public class SupplyRepository(InventoryDbContext context) : BaseRepository<Supply>(context), ISupplyRepository
{
    public async Task<Supply?> FindByNameAndSupplierAndOwnerIdAsync(string name, string supplier, int ownerId)
    {
        return await Context.Set<Supply>()
            .FirstOrDefaultAsync(s => s.SupplyName.ToLower() == name.ToLower() && s.Supplier.ToLower() == supplier.ToLower() && s.OwnerId == ownerId);
    }

    public async Task<IEnumerable<Supply>> ListByOwnerIdAsync(int ownerId)
    {
        return await Context.Set<Supply>()
            .Where(s => s.OwnerId == ownerId)
            .ToListAsync();
    }
}