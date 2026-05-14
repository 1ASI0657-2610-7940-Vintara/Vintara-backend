using Microsoft.EntityFrameworkCore;
using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Repositories;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace WinesoftPlatform.API.Inventory.Infrastructure.Persistence.Repositories;

public class SupplyRepository(AppDbContext context) : BaseRepository<Supply>(context), ISupplyRepository
{
    public async Task<Supply?> FindByNameAsync(string name)
    {
        return await Context.Set<Supply>()
            .FirstOrDefaultAsync(s => s.SupplyName.ToLower() == name.ToLower());
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await Context.Set<Supply>()
            .AnyAsync(s => s.SupplyName.ToLower() == name.ToLower());
    }
    
    public async Task<Supply?> FindByNameAndSupplierAsync(string name, string supplier)
    {
        return await Context.Set<Supply>()
            .FirstOrDefaultAsync(s => s.SupplyName.ToLower() == name.ToLower() && s.Supplier.ToLower() == supplier.ToLower());
    }
}